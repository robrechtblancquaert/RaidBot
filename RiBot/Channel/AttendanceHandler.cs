using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using RiBot.General;
using RiBot.Models;

namespace RiBot.Channel
{
    public enum Class { Guardian, Revenant, Warrior, Engineer, Ranger, Thief, Elementalist, Mesmer, Necromancer, None }
    class AttendanceHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Attending;
        public List<string> AcceptedCommands { get; } = new List<string> { "x", "!x", "!reset", "!roster" };

        // Dict of people attending the next raid, key: username, value; chosen class
        private Dictionary<string, Class> Usernames { get; set; }

        // Dict of the ammount of people wanted per class
        private Dictionary<Class, int> Roster { get; set; }

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised = false)
        {
            // Get the current attending users from the config file
            var configUsernames = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Usernames;
            this.Usernames = configUsernames ?? new Dictionary<string, Class>();

            // Get the curent roster from the config file
            var configRoster = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Roster;
            this.Roster = configRoster ?? new Dictionary<Class, int>();

            HandleCommand(command, isAuthorised);

            // Post the message
            await MessageHelper.UpdateMessage(postedMessage, FormMessage());

            // Update the config file
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Usernames = Usernames;
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Roster = Roster;
            Config.Instance.Write();

            return postedMessage;
        }

        /// <summary>
        /// Calls the appropriate method for each command
        /// </summary>
        /// <param name="command">The command to handle</param>
        /// <param name="isAuthorised">If the user is authorised</param>
        private void HandleCommand(Command command, bool isAuthorised)
        {
            // Extract the relevant portion of the command
            switch (command.FirstWord)
            {
                case "x":
                    AddUsername(command);
                    break;
                case "!x":
                    Usernames.Remove(command.Author.Username);
                    break;
                case "!reset":
                    if (isAuthorised) Usernames.Clear();
                    break;
                case "!roster":
                    if (isAuthorised) CreateRoster(command);
                    break;
            }
        }

        /// <summary>
        /// Adds a username, and their chosen class to the list of attending users
        /// </summary>
        /// <param name="command">The command containing the relevant information</param>
        private void AddUsername(Command command)
        {
            if (command.MessageRest.Length == 0) {
                Usernames[command.Author.Username] = Class.None;
                return;
            }
            List<Class> posClasses = MessageHelper.PossibleValues<Class>(command.MessageRest);

            // If more than one class is found that matches the command, assign class 'none'
            if(posClasses.Count == 1)
            {
                Usernames[command.Author.Username] = posClasses[0];
            } else
            {
                Usernames[command.Author.Username] = Class.None;
            }
        }

        /// <summary>
        /// Updates the roster with the given values
        /// </summary>
        /// <param name="command">The command containing the relevant information</param>
        private void CreateRoster(Command command)
        {
            if (command.MessageRest.Length == 0) return;
            var arguments = Argument.InString(command.MessageRest);
            foreach(var argument in arguments)
            {
                List<Class> posClasses = MessageHelper.PossibleValues<Class>(argument.Key);
                // If more than one class is found ignore this command
                if (posClasses.Count == 1)
                {
                    // try to convert the number given in command to an int
                    int posNumber = 0;
                    try
                    {
                        posNumber = int.Parse(argument.Value);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    Roster[posClasses[0]] = posNumber;
                }
            }
        }

        /// <summary>
        /// Formats the current information of this class into a string that can be posted in the channel
        /// </summary>
        /// <returns>The message to be posted as a string</returns>
        private string FormMessage()
        {
            string message = $"• Attending next raid [{Usernames.Count}]:\n";

            foreach (var classVal in (Class[])Enum.GetValues(typeof(Class)))
            {
                var myClass = Usernames.Where(x => x.Value == classVal);
                if (!Roster.ContainsKey(classVal)) Roster[classVal] = 0;
                if (myClass.Count() > 0 || Roster[classVal] > 0)
                {
                    message += "|\t" + myClass.Count() + "/" + Roster[classVal] + "\t" + classVal.ToString() + ":\t";
                    foreach(var pair in myClass)
                    {
                        message += pair.Key;
                        if(pair.Key != myClass.Last().Key)
                        {
                            message += ", ";
                        }
                    }
                    message += "\n";
                }
            }


            return message;
        }

        public string DefaultMessage()
        {
            string message = @"• Attending next raid [0]:";

            return message;
        }
    }
}
