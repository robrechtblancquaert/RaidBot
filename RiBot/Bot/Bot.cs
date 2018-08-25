using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using RiBot.Channel;

namespace RiBot
{
    /// <summary>
    /// Main class, handles start up
    /// </summary>
    public class Bot
    {
        // The client of discord
        private static DiscordSocketClient Client { get; set; }
        // A list of each of the channel handlers this class has started
        private static List<ChannelHandler> ChannelHandlers = new List<ChannelHandler>();
        // A timer to clean all channels up
        private static Timer CleanTimer;
        // Boolean to indicate if the daily reset has run
        private static bool HasReset = false;

        /// <summary>
        /// Main method of the program, configures the discord client
        /// </summary>
        /// <returns>Does not return, runs indefinetly</returns>
        public async Task Run()
        {
            Writer.Log("starting discord client configuration");

            // Initialise the config file and the discord client
            Client = new DiscordSocketClient();
            Config.Instance.Read();
            Config.Instance.Write();
            

#if ! DEBUG
            await Client.LoginAsync(TokenType.Bot, Config.Instance.ReleaseBotKey ); // RELEASE
#else
            await Client.LoginAsync(TokenType.Bot, Config.Instance.TestBotKey); // TEST
#endif
            await Client.StartAsync();

            // Call method to configure client once it is ready
            Client.Ready += Client_Ready;

            // Handle incoming messages
            DmHandler dmHandler = new DmHandler();
            Client.MessageReceived += async message =>
            {
                if(message.Author.Id != Client.CurrentUser.Id) Writer.Log("received message from " + message.Author + ": " + message.Content);

                var handler = ChannelHandlers.Where(x => x.Channel.Id == message.Channel.Id).SingleOrDefault();
                if (handler != null)
                {
                    handler.Handle(message);
                }
                else
                {
                    await dmHandler.Handle(message);
                }
            };

            // Start a timer to clean up the channels periodically
            CleanTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            CleanTimer.Elapsed += OnCleanEvent;
            CleanTimer.AutoReset = true;
            CleanTimer.Enabled = true;
            
            await Task.Delay(-1);
        }

        /// <summary>
        /// Starts a channel handler for each channel in the config, removes channels it can't access from config.
        /// </summary>
        private async Task Client_Ready()
        {
            List<ChannelConfig> toRemove = new List<ChannelConfig>();
            foreach (var channelconfig in Config.Instance.ChannelConfigs)
            {
                Writer.Log("started channel handler for channel with id: " + channelconfig.ChannelId);
                if (!(await StartChannelHandler(channelconfig)))
                {
                    toRemove.Add(channelconfig);
                }
            }
            foreach (var channelconfig in toRemove)
            {
                Config.Instance.ChannelConfigs.RemoveAll(x => x.ChannelId == channelconfig.ChannelId);
            }

            // Add bot to authorised users
            if (!Config.Instance.AuthUsersIds.Contains(Client.CurrentUser.Id))
            {
                Config.Instance.AuthUsersIds.Add(Client.CurrentUser.Id);
            }

            Config.Instance.Write();
        }

        /// <summary>
        /// Start a channel handler for a specific channel in discord
        /// </summary>
        /// <param name="channelconfig">The representation of a channel in discord</param>
        /// <returns>A bool depicting if the channel handler could be started</returns>
        public async static Task<bool> StartChannelHandler(ChannelConfig channelconfig)
        {
            // Create a handler for each type of message the channel should be able to process
            var handlers = new List<IMessageHandler>
            {
                new WelcomeHandler(),
                new AttendanceHandler(),
                new BorderHandler(),
                new ScheduleHandler(),
                new AnnouncementHandler()
            };

            // Try to get the channel object from  the channel id in the config
            var channel = Client.GetChannel(channelconfig.ChannelId) as IMessageChannel;

            // If the client couldn't get the channel, return false
            if(channel == null)
            {
                return false;
            }
            
            // Get all the messages that have been posted to the channel, remove any that have been deleted
            Dictionary<CommandType, IUserMessage> postedMessages = new Dictionary<CommandType, IUserMessage>();
            if (channelconfig.PostedMessages == null) channelconfig.PostedMessages = new Dictionary<CommandType, ulong>();

            // Check if a message has been deleted
            List<CommandType> toRemove = new List<CommandType>();
            foreach (var m in channelconfig.PostedMessages)
            {
                IUserMessage userMessage = null;
                try
                {
                    userMessage = await channel.GetMessageAsync(m.Value) as IUserMessage;
                }
                catch (Exception) { }

                if (userMessage == null)
                {
                    toRemove.Add(m.Key);
                }
                else
                {
                    postedMessages.Add(m.Key, userMessage);
                }
            }
            foreach (var type in toRemove)
            {
                Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channelconfig.ChannelId).Single().PostedMessages.Remove(type);
            }
            Config.Instance.Write();

            // If a handler doesn't have a message posted in the channel, post it's default message
            foreach (var h in handlers)
            {
                try
                {
                    if (postedMessages.Where(x => x.Key == h.MessageType).Count() == 0)
                    {
                        IUserMessage userMessage = await channel.SendMessageAsync(h.DefaultMessage());
                        postedMessages.Add(h.MessageType, userMessage);
                        Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channelconfig.ChannelId).Single().PostedMessages.Add(h.MessageType, userMessage.Id);
                    }
                }
                catch (Exception) { }
            }
            Config.Instance.Write();

            // Start a channel handler
            Channel.ChannelHandler handler = new Channel.ChannelHandler(Client.CurrentUser.Id, channel, handlers, postedMessages);
            ChannelHandlers.Add(handler);
            await Task.Run(() => handler.Run());

            return true;
        }

        /// <summary>
        /// Clean up a channel
        /// </summary>
        /// <param name="channel">The channel to clean up</param>
        private async static void Clean(IMessageChannel channel)
        {
            // Remove all announcements that have expired from both the channel and the config (where necessary)
            var announcements = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Announcements;
            List<ulong> toRemove = new List<ulong>();
            foreach (var a in announcements)
            {
                var message = await channel.GetMessageAsync(a.Key);
                if (message == null)
                {
                    toRemove.Add(a.Key);
                }
                else
                {
                    if (a.Value < DateTime.Now)
                    {
                        List<IMessage> toDelete = new List<IMessage>();
                        toDelete.Add(message);
                        await channel.DeleteMessagesAsync(toDelete);
                        toRemove.Add(a.Key);
                    }
                }
            }
            foreach (var remove in toRemove)
            {
                announcements.Remove(remove);
            }
            Config.Instance.Write();
        }

        /// <summary>
        /// Starts the clean method for all channels, when triggered by the timer.
        /// </summary>
        private static void OnCleanEvent(Object source, ElapsedEventArgs e)
        {
            Writer.Log("cleaning up channels");

            foreach (var channelconfig in Config.Instance.ChannelConfigs)
            {
                var channel = Client.GetChannel(channelconfig.ChannelId) as IMessageChannel;

                Clean(channel);
            }

            // Reset at night
            if(DateTime.Now.ToLocalTime().Hour == 1 && !HasReset)
            {
                Writer.Log("resetting channel");
                foreach (var handler in ChannelHandlers)
                {
                    // Only reset attendance if there has been a raid
                    var raidDays = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == handler.Channel.Id).Single().Schedule.Keys.ToArray();
                    DayOfWeek yesterday = DateTime.Now.AddDays(-1).DayOfWeek;
                    if(raidDays.Contains(yesterday))
                    {
                        handler.Channel.SendMessageAsync("!reset");
                    }
                    // Send schedule message to update it to a indicate today
                    handler.Channel.SendMessageAsync("!schedule");
                } 

                HasReset = true;
                // Reinitialise the writer to start a new log file
                Writer.Initialise();
            }

            if(DateTime.Now.ToLocalTime().Hour == 2 && HasReset)
            {
                HasReset = false;
            }
        }
    }
}

