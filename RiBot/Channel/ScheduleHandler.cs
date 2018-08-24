using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace RiBot.Channel
{
    class ScheduleHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Schedule;

        public List<string> AcceptedCommands { get; } = new List<string> { "!schedule" };


        public Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            throw new NotImplementedException();
        }

        public string DefaultMessage()
        {
            return "Schedule: Not set";
        }
    }
}
