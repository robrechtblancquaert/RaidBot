using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace RiBot.Channel
{
    class ScheduleHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Schedule;

        public List<string> AcceptedCommands { get; } = new List<string> { "!schedule", "!daily" };

        // A weekly schedule represented by a dict, key: day, value: time of day
        private Dictionary<DayOfWeek, TimeSpan> Schedule = new Dictionary<DayOfWeek, TimeSpan>();

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised)
        {
            // Only authorised users
            if (!isAuthorised) return postedMessage;

            // Get the current schedule from the config file
            var configSchedule = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Schedule;
            this.Schedule = configSchedule ?? new Dictionary<DayOfWeek, TimeSpan>();
            
            HandleCommand(command);

            // Post the message
            await MessageHelper.UpdateMessage(postedMessage, FormMessage());

            // Update the config file
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Schedule = Schedule;
            Config.Instance.Write();

            return postedMessage;
        }

        /// <summary>
        /// Calls the appropriate method for each command
        /// </summary>
        /// <param name="command">The command to handle</param>
        private void HandleCommand(Command command)
        {
            // Extract the relevant portion of the command
            switch (command.FirstWord)
            {
                case "!schedule":
                    CreateSchedule(command);
                    break;
                case "!daily":
                    break;
            }
        }

        /// <summary>
        /// Creates a schedule based on given commands
        /// </summary>
        /// <param name="command">A string containing Arguments to set the schedule</param>
        private void CreateSchedule(Command command)
        {
            if (command.MessageRest.Length > 0) return;
            if(command.MessageRest == "reset")
            {
                Schedule = new Dictionary<DayOfWeek, TimeSpan>();
                return;
            }

            var arguments = Argument.InString(command.MessageRest);
            foreach (var argument in arguments)
            {
                List<DayOfWeek> posDays = MessageHelper.PossibleValues<DayOfWeek>(command.MessageRest);

                // If more than one day is found ignore this command
                if (posDays.Count == 1)
                {
                    if (argument.Value == "reset")
                    {
                        Schedule.Remove(posDays[0]);
                    }
                    else
                    {
                        // try to get the datetime given in the command
                        TimeSpan timeSpan = new TimeSpan();
                        try
                        {
                            timeSpan = TimeSpan.ParseExact(argument.Value, "hh\\:mm", CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        Schedule[posDays[0]] = timeSpan;
                    }
                }
            }
        }

        /// <summary>
        /// Formats the current information of this class into a string that can be posted in the channel
        /// </summary>
        /// <returns>The message to be posted as a string</returns>
        private string FormMessage()
        {
            if(Schedule.Count == 0)
            {
                return "• Schedule: Not set";
            }
            string message = @"• Schedule:
`";

            var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            foreach(var day in days)
            {
                if(DateTime.Now.DayOfWeek == day)
                {
                    message += "| -x- ";
                }
                else
                {
                    message += "|     ";
                }
            }

            message += @"|`
`| Mon | Tue | Wed | Thu | Fri | Sat | Sun |`
`";
            foreach (var en in days)
            {
                if(Schedule.Keys.Contains(en))
                {
                    message += $"|{Schedule[en].Hours + ":" + Schedule[en].Minutes.ToString("D2")}";
                } else
                {
                    message += "|     ";
                }
            }

            return message + "|`";
        }

        public string DefaultMessage()
        {
            return "• Schedule: Not set";
        }
    }
}
