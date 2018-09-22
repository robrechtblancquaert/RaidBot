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
    class AttendanceHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Attending;
        public ChannelConfig ChannelConfig { get; }
        public List<string> AcceptedCommands { get; } = new List<string> { "x", "!x", "!reset", "!roster" };

        // Dict of people attending the next raid, key: username, value: chosen class
        private Dictionary<string, string> Usernames { get; set; }

        // Dict of the ammount of people wanted per class
        private Dictionary<string, int> Roster { get; set; }

        public AttendanceHandler(ChannelConfig channelConfig)
        {
            this.ChannelConfig = channelConfig;
        }

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised = false)
        {
            // Get the current attending users from the config file
            this.Usernames = ChannelConfig.ChannelData.Usernames;

            // Get the curent roster from the config file
            this.Roster = ChannelConfig.ChannelData.Roster;

            HandleCommand(command, isAuthorised);

            // Post the message
            await MessageHelper.UpdateMessage(postedMessage, FormMessage());

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
            List<string> classes = ChannelConfig.ClassTypes;

            if (command.MessageRest.Length == 0) {
                Usernames[command.Author.Username] = classes.Last();
                return;
            }
            List<string> posClasses = MessageHelper.PossibleValues(classes, command.MessageRest);

            // If more than one class is found that matches the command, assign class 'none'
            if(posClasses.Count == 1)
            {
                Usernames[command.Author.Username] = posClasses[0];
            } else
            {
                Usernames[command.Author.Username] = classes.Last();
            }
        }

        /// <summary>
        /// Updates the roster with the given values
        /// </summary>
        /// <param name="command">The command containing the relevant information</param>
        private void CreateRoster(Command command)
        {
            if (command.MessageRest.Length == 0) return;

            List<string> classes = ChannelConfig.ClassTypes;

            var arguments = Argument.InString(command.MessageRest);
            foreach(var argument in arguments)
            {
                List<string> posClasses = MessageHelper.PossibleValues(classes, argument.Key);
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

            List<string> classes = ChannelConfig.ClassTypes;

            foreach (var classVal in classes)
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
