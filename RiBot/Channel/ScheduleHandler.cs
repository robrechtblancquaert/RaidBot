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

        public List<string> AcceptedCommands { get; } = new List<string> { "!schedule" };

        // A weekly schedule represented by a dict, key: day, value: time of day
        private Dictionary<DayOfWeek, TimeSpan> Schedule = new Dictionary<DayOfWeek, TimeSpan>();

        public async Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            // Only authorised users
            if (!Config.Instance.AuthUsersIds.Contains(command.Author.Id))
            {
                return message;
            }

            // Get the current schedule from the config file
            var configSchedule = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Schedule;
            if (configSchedule != null)
            {
                this.Schedule = configSchedule;
            }
            else
            {
                this.Schedule = new Dictionary<DayOfWeek, TimeSpan>();
            }

            HandleCommand(command);

            IUserMessage postedMessage = message;

            // Post the message
            if (message == null)
            {
                postedMessage = await channel.SendMessageAsync(FormMessage());
            }
            else
            {
                try
                {
                    await message.ModifyAsync(x => x.Content = FormMessage());
                }
                catch (Exception)
                {
                    postedMessage = await channel.SendMessageAsync(FormMessage());
                }
            }

            // Update the config file
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Schedule = Schedule;
            Config.Instance.Write();

            return postedMessage;
        }

        /// <summary>
        /// Calls the appropriate method for each command
        /// </summary>
        /// <param name="command">The command to handle</param>
        private void HandleCommand(IMessage command)
        {
            // Extract the relevant portion of the command
            string commandString = (command.Content.IndexOf(' ') == -1) ? command.Content.ToLower() : command.Content.Substring(0, command.Content.IndexOf(' ')).ToLower();
            switch (commandString)
            {
                case "!schedule":
                    CreateSchedule(command);
                    break;
            }
        }

        /// <summary>
        /// Creates a schedule based on given commands
        /// </summary>
        /// <param name="command">A string containing Arguments to set the schedule</param>
        private void CreateSchedule(IMessage command)
        {
            string content = command.Content;
            // Check valid structure of command
            if (content.Length >= "!schedule [mon:20:00]".Length && content.IndexOf(' ') != -1)
            {
                var arguments = Argument.InString(content);
                foreach (var argument in arguments)
                {
                    List<DayOfWeek> posDays = new List<DayOfWeek>();
                    // Find every day that starts with the same letters as given in the command, and add them to the list of possible days
                    foreach (var en in (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                    {
                        if (en.ToString().Length >= argument.Key.Length)
                        {
                            if (en.ToString().ToLower().Substring(0, argument.Key.Length) == argument.Key)
                            {
                                posDays.Add(en);
                            }
                        }
                    }
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
            else
            {
                // Reset the schedule
                if (content.Length >= "!schedule reset".Length && content.IndexOf(' ') != -1)
                {
                    if(content.Substring(content.IndexOf(' ') + 1).ToLower() == "reset")
                    {
                        Schedule = new Dictionary<DayOfWeek, TimeSpan>();
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
