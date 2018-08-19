using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace RiBot.Channel
{
    class WelcomeHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Welcome;
        public List<string> AcceptedCommands { get; } = new List<string> { "!welcome" };

        public async Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            IUserMessage postedMessage = message;

            if (!Config.Instance.AuthUsersIds.Contains(command.Author.Id)) return postedMessage;

            string content = command.Content;

            // Check that the command has the correct structure
            string welcome = null;
            if (content.Length > "!welcome a".Length && content.IndexOf(' ') != -1)
            {
                welcome = content.Substring(content.IndexOf(' ') + 1).ToLower();
            }
            if (welcome == null) return postedMessage;

            // Create and post the embeded message
            var eb = new EmbedBuilder();
            eb.WithDescription(welcome);
            eb.Color = Color.Green;
            if (message == null)
            {
                postedMessage = await channel.SendMessageAsync("***Raid Attendance***", false, eb);
            }
            else
            {
                try
                {
                    await message.ModifyAsync(x => { x.Embed = eb.Build(); x.Content = "***Raid Attendance***"; });
                }
                catch (Exception)
                {
                    postedMessage = await channel.SendMessageAsync("***Raid Attendance***", false, eb);
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
