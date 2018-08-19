using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace RiBot.Channel
{
    class BorderHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Border;
        public List<string> AcceptedCommands { get; } = new List<string> { "!border", "!reset" };

        // List of possible values for border
        private List<string> BorderColours { get; } = new List<string> { "Blue", "Red", "Green", "None", "OS", "EotM" };

        public async Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            IUserMessage postedMessage = message;

            if (!Config.Instance.AuthUsersIds.Contains(command.Author.Id)) return postedMessage;

            string content = command.Content;

            // Check if the structure of the command is correct, "!reset" command will give an invalid structure, and so the border will correctly be set to "None"
            string border = null;
            if (content.Length > "!border b".Length && content.IndexOf(' ') != -1)
            {
                string borderVal = content.Substring(content.IndexOf(' ') + 1).ToLower();
                border = BorderColours.Where(x => x.ToLower() == borderVal).SingleOrDefault();
            }
            if (border == null) border = "None";

            // Post the message
            if (message == null)
            {
                postedMessage = await channel.SendMessageAsync($"• Border: {border}");
            }
            else
            {
                try
                {
                    await message.ModifyAsync(x => x.Content = $"• Border: {border}");
                }
                catch (Exception)
                {
                    postedMessage = await channel.SendMessageAsync($"• Border: {border}");
                }
            }

            return postedMessage;
        }

        public string DefaultMessage()
        {
            return $"• Border: None";
        }
    }
}
