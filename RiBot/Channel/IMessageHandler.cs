using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiBot.Channel
{
    /// <summary>
    /// Handles a specific type of message
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// The type this class is responisble for
        /// </summary>
        CommandType MessageType { get; }
        /// <summary>
        /// The commands this class accepts
        /// </summary>
        List<string> AcceptedCommands { get; }

        /// <summary>
        /// Handle a command
        /// </summary>
        /// <param name="message">The current message posted by this class in  the channel</param>
        /// <param name="command">The message received by the bot</param>
        /// <param name="channel">The channel in which the command was received</param>
        /// <returns>An updated message posted in the channel</returns>
        Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel);

        /// <summary>
        /// Creates a default message, to be posted in the channel if there is no current posted message
        /// </summary>
        /// <returns>The default message</returns>
        string DefaultMessage();
    }
}
