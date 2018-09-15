using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiBot.General;
using RiBot.Models;

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
        private ChannelConfig ChannelConfig { get; }

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
            this.ChannelConfig = Config.Instance.GetChannelConfig(channel);
        }

        /// <summary>
        /// Use this method to clear any unhandled messages in the channel
        /// </summary>
        public async Task Run()
        {
            var messages = await Channel.GetMessagesAsync().Flatten();
            foreach (var m in messages)
            {
                await Handle(Command.FormCommand(m));
            }

        }

        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="m">The message to handle</param>
        public async Task Handle(Command command)
        {
            // Is the author authorised in this channel
            bool authorised = ChannelConfig.AuthUsersIds.Contains(command.Author.Id) || (command.Author.Id == this.BotId);

            // Do not handle the message if it is from the bot itself, unless it is a request to reset
            if (command.Author.Id != BotId || command.FirstWord == "!reset" || command.FirstWord == "!daily")
            {
                // Let each handler handle the received command
                foreach (var handler in MessageHandlers)
                {
                    if (handler.AcceptedCommands.Contains(command.FirstWord))
                    {
                        var postedMessage = await handler.Handle(PostedMessages.Where(x => x.Key == handler.MessageType).Single().Value, command, authorised);
                        PostedMessages[handler.MessageType] = postedMessage;

                    }
                }

                // Update the config file with the new messages
                Dictionary<CommandType, ulong> forConfig = new Dictionary<CommandType, ulong>();
                foreach (var x in PostedMessages)
                {
                    forConfig.Add(x.Key, x.Value.Id);
                }
                ChannelConfig.ChannelData.PostedMessages = forConfig;

                // Delete the message after it has been handled
                List<IMessage> toDelete = new List<IMessage>
                {
                    command.Message
                };
                await Channel.DeleteMessagesAsync(toDelete);
            }
        }

    }
}
