using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

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

        public async Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            // Get the current attending users from the config file
            var configUsernames = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Usernames;
            if(configUsernames != null)
            {
                this.Usernames = configUsernames;
            } else
            {
                this.Usernames = new Dictionary<string, Class>();
            }

            // Get the curent roster from the config file
            var configRoster = Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Roster;
            if(configRoster != null)
            {
                this.Roster = configRoster;
            } else
            {
                this.Roster = new Dictionary<Class, int>();
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
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Usernames = Usernames;
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Roster = Roster;
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
                case "x":
                    AddUsername(command);
                    break;
                case "!x":
                    Usernames.Remove(command.Author.Username);
                    break;
                case "!reset":
                    if (Config.Instance.AuthUsersIds.Contains(command.Author.Id)) Usernames.Clear();
                    break;
                case "!roster":
                    CreateRoster(command);
                    break;
            }
        }

        /// <summary>
        /// Adds a username, and their chosen class to the list of attending users
        /// </summary>
        /// <param name="command">The command containing the relevant information</param>
        private void AddUsername(IMessage command)
        {
            string content = command.Content;
            List<Class> posClasses = new List<Class>();
            // Check valid structure of command
            if (content.Length >= "x g".Length && content.IndexOf(' ') != -1)
            {
                string posClass = content.Substring(content.IndexOf(' ') + 1).ToLower();
                // Find every class that starts with the same letters as given in the command, and add them to the list of possible classes
                foreach(var en in (Class[])Enum.GetValues(typeof(Class)))
                {
                    try
                    {
                        if (en.ToString().ToLower().Substring(0, posClass.Length) == posClass)
                        {
                            posClasses.Add(en);
                        }
                    }
                    catch (Exception) { }
                }

                // If more than one class is found that matches the command, assign class 'none'
                if(posClasses.Count == 1)
                {
                    Usernames[command.Author.Username] = posClasses[0];
                } else
                {
                    Usernames[command.Author.Username] = Class.None;
                }
            } else
            {
                Usernames[command.Author.Username] = Class.None;
            }
        }

        /// <summary>
        /// Updates the roster with the given values
        /// </summary>
        /// <param name="command">The command containing the relevant information</param>
        private void CreateRoster(IMessage command)
        {
            //structure: !roster [g:3][reven:3]...

            string content = command.Content;
            // Check valid structure of command
            if (content.Length >= "!roster [g:1]".Length && content.IndexOf(' ') != -1)
            {
                while(content.IndexOf('[') != -1)
                {
                    // Get a specif class number assignement from the command
                    int startIndex = content.IndexOf('[');
                    int endIndex = content.IndexOf(']');
                    if (endIndex == -1) return;
                    string assignement = content.Substring(startIndex, endIndex - startIndex + 1);

                    int middleIndex = assignement.IndexOf(':');
                    if (middleIndex == -1) return;
                    string aClass = assignement.Substring(1, middleIndex - 1);
                    string aNumber = assignement.Substring(middleIndex + 1, assignement.Length - aClass.Length - 3);

                    List<Class> posClasses = new List<Class>();
                    // Find every class that starts with the same letters as given in the command, and add them to the list of possible classes
                    foreach (var en in (Class[])Enum.GetValues(typeof(Class)))
                    {
                        try
                        {
                            if (en.ToString().ToLower().Substring(0, aClass.Length) == aClass)
                            {
                                posClasses.Add(en);
                            }
                        }
                        catch (Exception) { }
                    }
                    // If more than one class is found ignore this command
                    if (posClasses.Count == 1)
                    {
                        // try to convert the number given in command to an int
                        int posNumber = 0;
                        try
                        {
                            posNumber = int.Parse(aNumber);
                        } catch(Exception)
                        {
                            return;
                        }
                        Roster[posClasses[0]] = posNumber;
                    }

                    // Remove the processed assingement from content
                    content = content.Remove(0, content.IndexOf(']') + 1);
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
            string message = $@"• Attending next raid [0]:";

            return message;
        }
    }
}
