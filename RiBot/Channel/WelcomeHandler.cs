using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using RiBot.General;
using RiBot.Models;

namespace RiBot.Channel
{
    class WelcomeHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Welcome;
        public List<string> AcceptedCommands { get; } = new List<string> { "!welcome" };

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised = false)
        {
            if (!isAuthorised) return postedMessage;

            if (command.MessageRest.Length == 0) return postedMessage;

            // Create and post the embeded message
            var eb = new EmbedBuilder();
            eb.WithDescription(command.MessageRest);
            eb.Color = Color.Green;
            if (postedMessage == null)
            {
                postedMessage = await command.Channel.SendMessageAsync("***Raid Attendance***", false, eb);
            }
            else
            {
                try
                {
                    await postedMessage.ModifyAsync(x => { x.Embed = eb.Build(); x.Content = "***Raid Attendance***"; });
                }
                catch (Exception)
                {
                    postedMessage = await command.Channel.SendMessageAsync("***Raid Attendance***", false, eb);
                }
            }

            return postedMessage;
        }

        public string DefaultMessage()
        {
            return "***Raid Attendance***";
        }
    }
}
