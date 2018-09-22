using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using RiBot.General;
using RiBot.Models;

namespace RiBot.Channel.Handlers
{
    class BorderHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Border;
        public ChannelConfig ChannelConfig { get; }
        public List<string> AcceptedCommands { get; } = new List<string> { "!border", "!reset" };

        // List of possible values for border
        private List<string> BorderColours { get; } = new List<string> { "Blue", "Red", "Green", "None", "OS", "EotM" };

        public BorderHandler(ChannelConfig channelConfig)
        {
            this.ChannelConfig = channelConfig;
        }

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised = false)
        {
            if (!isAuthorised) return postedMessage;

            string border = "None";
            if(command.MessageRest.Length != 0)
            {
                List<string> posBorders = MessageHelper.PossibleValues(BorderColours, command.MessageRest);
                if(posBorders.Count() == 1)
                {
                    border = posBorders[0];
                }
            }

            await MessageHelper.UpdateMessage(postedMessage, $"• Border: {border}");

            return postedMessage;
        }

        public string DefaultMessage()
        {
            return "• Border: None";
        }
    }
}
