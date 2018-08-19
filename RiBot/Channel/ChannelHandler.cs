using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiBot.Channel
{
    /// <summary>
    /// Handlers messages for a specific channel
    /// </summary>
    public class ChannelHandler
    {
        public IMessageChannel Channel { get; }
        public IDictionary<CommandType, IUserMessage> PostedMessages { get; set; }
        public List<IMessageHandler> MessageHandlers { get; set; }
        private ulong BotId { get; }

        /// <summary>
        /// Create a ChannelHandler
        /// </summary>
        /// <param name="botId">The id of the bot</param>
        /// <param name="channel">The channel this class should handle</param>
        /// <param name="messageHandlers">A list of all the handlers for messages this channel can receive</param>
        /// <param name="postedMessages">A list of all the messages that have been posted to this channel</param>
        public ChannelHandler(ulong botId, IMessageChannel channel, List<IMessageHandler> messageHandlers, IDictionary<CommandType, IUserMessage> postedMessages = null)
        {
            this.BotId = botId;
            this.Channel = channel;
            this.MessageHandlers = messageHandlers;
            this.PostedMessages = (postedMessages != null) ? postedMessages : new Dictionary<CommandType, IUserMessage>();
        }

        /// <summary>
        /// Use this method to clear any unhandled messages in the channel
        /// </summary>
        public async void Run()
        {
            var messages = await Channel.GetMessagesAsync().Flatten();
            foreach (var m in messages)
            {
                Handle(m);
            }

        }

        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="m">The message to handle</param>
        public async void Handle(IMessage m)
        {
            // Do not handle the message if it is from the bot itself, unless it is a request to reset
            if (m.Author.Id != BotId || m.Content == "!reset")
            {
                // Extract the command from the received message
                string command = (m.Content.IndexOf(' ') == -1) ? m.Content.ToLower() : m.Content.Substring(0, m.Content.IndexOf(' ')).ToLower();

                // Let each handler handle the received command
                foreach (var handler in MessageHandlers)
                {
                    if (handler.AcceptedCommands.Contains(command))
                    {
                        var postedMessage = await handler.Handle(PostedMessages.Where(x => x.Key == handler.MessageType).Single().Value, m, Channel);
                        PostedMessages[handler.MessageType] = postedMessage;

                    }
                }

                // Update the config gile with the new messages
                Dictionary<CommandType, ulong> forConfig = new Dictionary<CommandType, ulong>();
                foreach (var x in PostedMessages)
                {
                    forConfig.Add(x.Key, x.Value.Id);
                }
                Config.Instance.ChannelConfigs.Where(x => x.ChannelId == Channel.Id).Single().PostedMessages = forConfig;
                Config.Instance.Write();

                // Delete the message after it has been handled
                List<IMessage> toDelete = new List<IMessage>();
                toDelete.Add(m);
                await Channel.DeleteMessagesAsync(toDelete);
            }
        }

    }
}
