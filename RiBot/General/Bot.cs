﻿using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using RiBot.Channel;
using RiBot.Models;

namespace RiBot.General
{
    /// <summary>
    /// Main class, handles start up
    /// </summary>
    public class Bot
    {
        // The client of discord
        public static DiscordSocketClient Client { get; set; }
        // A list of each of the channel handlers this class has started
        private static List<ChannelHandler> ChannelHandlers = new List<ChannelHandler>();
        // A timer to periodically clean all channels up
        private static Timer CleanTimer;
        // Boolean to indicate if the daily reset has run
        private static bool HasReset = false;

        /// <summary>
        /// Main method of the program, runs indefinetly
        /// </summary>
        /// <returns>Does not return, runs indefinetly</returns>
        public async Task Run()
        {
            Writer.Log("starting discord RaidBot client");

            // Initialise the config file and the discord client
            Client = new DiscordSocketClient();
            Config.Instance.Read();
            Config.Instance.Write();

            await this.Client_Setup();

            await Task.Delay(-1);
        }

        /// <summary>
        /// Setup the client and assign methods
        /// </summary>
        public async Task Client_Setup()
        {
#if !DEBUG
            await Client.LoginAsync(TokenType.Bot, Config.Instance.General.ReleaseBotKey ); // RELEASE
#else
            await Client.LoginAsync(TokenType.Bot, Config.Instance.General.TestBotKey); // TEST
#endif
            await Client.StartAsync();

            // Call method to configure client once it is ready
            Client.Ready += Client_Ready;

            Client.Disconnected += Client_Disconected;

            // Handle incoming messages
            DmHandler dmHandler = new DmHandler();
            Client.MessageReceived += async message =>
            {
                var handler = ChannelHandlers.Where(x => x.Channel.Id == message.Channel.Id).SingleOrDefault();
                if (handler != null)
                {
                    if (message.Author.Id != Client.CurrentUser.Id) Writer.Log("received message from " + message.Author + ": " + message.Content);
                    handler.Handle(Command.FormCommand(message));
                }
                else
                {
                    if ((message.Channel.GetType() == typeof(SocketDMChannel)) && (message.Author.Id != Client.CurrentUser.Id))
                    {
                        Writer.Log("received dm from " + message.Author + ": " + message.Content);
                        await dmHandler.Handle(Command.FormCommand(message));
                    }
                }
            };

            // Start a timer to clean up the channels periodically
            CleanTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            CleanTimer.Elapsed += OnCleanEvent;
            CleanTimer.AutoReset = true;
            CleanTimer.Enabled = true;
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

            Config.Instance.Write();
        }

        /// <summary>
        /// Handles a loss of connection with the client
        /// </summary>
        /// <param name="exception">Exception given on disconnection</param>
        private async Task Client_Disconected(Exception exception)
        {
            Writer.Log($"Client disconnected: {exception.Message}");

            // Dispose of used resources
            ChannelHandlers.Clear();
            CleanTimer.Dispose();
            try
            {
                Client.Dispose();
            }
            catch(Exception e)
            {
                Writer.Log("Could no dispose of client.");
            }

            // Try to reconnect
            bool disconnected = true;
            while(disconnected)
            {
                try
                {
                    await this.Client_Setup();
                    disconnected = false;
                }
                catch (Exception e)
                {
                    Writer.Log("Could not start client.");
                }
                await Task.Delay(TimeSpan.FromSeconds(10).Milliseconds);
            }

            return;
        }

        /// <summary>
        /// Start a channel handler for a specific channel in discord
        /// </summary>
        /// <param name="channelconfig">The representation of a channel in discord</param>
        /// <param name="channel">If you already have an instanc eof the channel to handle you can pass it as a parameter</param>
        /// <returns>A bool depicting if the channel handler could be started</returns>
        public async static Task<bool> StartChannelHandler(ChannelConfig channelconfig, IMessageChannel channel = null)
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
            if(channel == null)
            {
                channel = Client.GetChannel(channelconfig.ChannelId) as IMessageChannel;
            }
            else
            {
                channelconfig = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single();
            }

            // If the client couldn't get the channel, return false
            if(channel == null)
            {
                Config.Instance.DeleteConfig(channelconfig);
            }
            
            // Get all the messages that have been posted to the channel, remove any that have been deleted
            Dictionary<CommandType, IUserMessage> postedMessages = new Dictionary<CommandType, IUserMessage>();
            if (channelconfig.ChannelData.PostedMessages == null) channelconfig.ChannelData.PostedMessages = new Dictionary<CommandType, ulong>();

            // Check if a message has been deleted
            List<CommandType> toRemove = new List<CommandType>();
            foreach (var m in channelconfig.ChannelData.PostedMessages)
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
                Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channelconfig.ChannelId).Single().ChannelData.PostedMessages.Remove(type);
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
                        Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channelconfig.ChannelId).Single().ChannelData.PostedMessages.Add(h.MessageType, userMessage.Id);
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
            var announcements = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().ChannelData.Announcements;
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
                if(channel != null)
                {
                    Clean(channel);
                }
                else
                {
                    Config.Instance.DeleteConfig(channelconfig);
                }
            }

            // Reset at night
            if(DateTime.Now.ToLocalTime().Hour == 1 && !HasReset)
            {
                Writer.Log("resetting channel");
                foreach (var handler in ChannelHandlers)
                {
                    try
                    {
                        // Only reset attendance if there has been a raid
                        var raidDays = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == handler.Channel.Id).Single().ChannelData.Schedule.Keys.ToArray();
                        DayOfWeek yesterday = DateTime.Now.AddDays(-1).DayOfWeek;
                        if (raidDays.Contains(yesterday))
                        {
                            handler.Channel.SendMessageAsync("!reset");
                        }
                    }
                    catch (Exception) { }
                    

                    // Message that reset has passed for handlers that need it
                    handler.Channel.SendMessageAsync("!daily");
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

