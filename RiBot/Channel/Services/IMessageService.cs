using RiBot.General;
using RiBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RiBot.Channel.Services
{
    /// <summary>
    /// Processes a specific type of message, but is not responsible for a message posted by the bot
    /// </summary>
    interface IMessageService
    {
        /// <summary>
        /// The type this class is responisble for
        /// </summary>
        CommandType MessageType { get; }

        /// <summary>
        /// The data context
        /// </summary>
        ChannelConfig ChannelConfig { get; }

        /// <summary>
        /// The commands this class accepts
        /// </summary>
        List<string> AcceptedCommands { get; }

        /// <summary>
        /// Provide a service as a response to a command
        /// </summary>
        /// <param name="command">The message received by the bot, stored in command object</param>
        /// <param name="isAuthorised">True if user is authorised in the channel, false otherwise</param>
        Task Service(Command command, bool isAuthorised = false);
    }
}
