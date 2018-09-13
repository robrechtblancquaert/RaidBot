using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiBot
{
    public class Command
    {
        public string FirstWord { get; set; }
        public string MessageRest { get; set; }
        public IMessage Message { get; set; }
        public IUser Author { get; set; }
        public IMessageChannel Channel { get; set; }

        public static Command FormCommand(IMessage message)
        {
            Command command = new Command()
            {
                FirstWord = (message.Content.IndexOf(' ') == -1) ? message.Content.ToLower() : message.Content.Substring(0, message.Content.IndexOf(' ')).ToLower(),
                Author = message.Author,
                Channel = message.Channel,
                Message = message
            };
            command.MessageRest = (message.Content.Length > command.FirstWord.Length) ? message.Content.Substring(command.FirstWord.Length + 1) : "";

            return command;
        }
    }
}
