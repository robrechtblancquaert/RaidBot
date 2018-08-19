using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiBot
{
    /// <summary>
    /// The class that will handle any messages sent to the bot in dm
    /// </summary>
    public class DmHandler
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message">The message to handle</param>
        public async Task Handle(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            if (message.Channel.GetType() != typeof(SocketDMChannel)) return;
            string command = message.Content;
            if (command.IndexOf(' ') != -1) command = command.Substring(0, command.IndexOf(' '));
            switch (command)
            {
                case "!help":
                    await SendHelp(message);
                    break;
                case "!config":
                    await SendConfig(message);
                    break;
                case "!auth":
                    await AuthId(message);
                    break;
                case "!addchannel":
                    await AddChannel(message);
                    break;
                default:
                    await message.Channel.SendMessageAsync("Unrecognised command");
                    break;
            }
        }

        /// <summary>
        /// Sends a list of commands to the person sending the message, more commands if that person is an authorised user
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task SendHelp(SocketMessage message)
        {
            // A couple of giant strings that contain all the info of the possible commands (discord has max message size, so split them up)
            string help = @"
List of commands in DM:
    | `!help` => Displays this message.
/
List of commands in channel:
    | `x {class}` => Register that you are attending the next raid. Replace `{class}` with an identifiable classname.
     This means you only need to type as many letters of the class name as to avoid there being any other class names that start with the same letters.
```x guardian
x g
x r => Will not work because both 'ranger' and 'revenant' start with r```
     Possible values for class: `[Guardian, Revenant, Warrior, Engineer, Ranger, Thief, Elementalist, Mesmer, Necromancer, None]`
     If an invalid class or no class is given, you will be registered under `None`
    | `!x` => Remove yourself from the attendance list for next raid.
";
            await message.Channel.SendMessageAsync(help);
            if (Config.Instance.AuthUsersIds.Contains(message.Author.Id))
            {
                string adminhelp = @"/
Commands for admins in DM:
    | `!config` => Returns a json of the current bot configuration.
    | `!auth {userid}` => Makes a user an admin. `{userid}` is the id of the user you want to make an admin. Get the id by right-clicking on a user with developer mode enabled.
    | `!addchannel {channelid}` => Tell the bot to take over a channel, where `{channelid}` is that channel's id. The id is obtained by right clicking on the text-channel when developer mode is enabled.
     WARNING: THIS WILL DESTROY ANY MESSAGES IN THAT CHANNEL!
/
Commands for admins in channel:
    | `!border {color}` => Changes the border message to display the given color.
     Possible values for {color}: `[Blue, Red, Green, OS, Eotm]` (capital letter not required)
    | `!reset` => Reset attendance and border colour. This command will automatically be called every night at 3.
    | `!welcome {message}` => Replaces the welcome message by `{message}`
    | `!announce {announcement} {-h hh:mm}` => Places the `{announcement}` in the channel. Optionall you can add `{-h hh:mm}` to the end to make it expire after a given time, default is 4 hours.
```!announce something -h 00:05 => Will expire after 5 minutes.
!announce something one -h 99:30 => Will expire after 99 hours and 30 minutes.
!announce something that is two => Will expire after 4 hours.```
";
                await message.Channel.SendMessageAsync(adminhelp);

                string adminhelp_2 = @"/
    | `!roster {values}` => Assigns a specific roster to attendance, {values} consists of multiple assignements formed as such: `[{class}:{number}]`.
     Class is assigned the same way as in the 'x {class}` command, so you don't have to type the whole class name.
```!roster [g:3][war:3][mes:1]```
";
                await message.Channel.SendMessageAsync(adminhelp_2);
            }
            
        }

        /// <summary>
        /// If the user is authorised, send them a copy of the current config.json
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task SendConfig(SocketMessage message)
        {
            if (!Config.Instance.AuthUsersIds.Contains(message.Author.Id)) return;
            string json = JsonConvert.SerializeObject(Config.Instance, Formatting.Indented);
            await message.Channel.SendMessageAsync(json);
        }

        /// <summary>
        /// If the user is authorised, authorise, or remove authrisation from, an other user
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task AuthId(SocketMessage message)
        {
            if (!Config.Instance.AuthUsersIds.Contains(message.Author.Id)) return;
            string id = null;
            if ((message.Content.IndexOf(' ') != -1) && message.Content.Length > "!auth 1".Length) id = message.Content.Substring(message.Content.IndexOf(' ') + 1);
            if (id == null)
            {
                await message.Channel.SendMessageAsync("Invalid id");
            }
            else
            {
                try
                {
                    if (Config.Instance.AuthUsersIds.Contains(Convert.ToUInt64(id)))
                    {
                        Config.Instance.AuthUsersIds.Remove(Convert.ToUInt64(id));
                        await message.Channel.SendMessageAsync("Removed Authorisation " + Convert.ToUInt64(id));
                    }
                    else
                    {
                        Config.Instance.AuthUsersIds.Add(Convert.ToUInt64(id));
                        await message.Channel.SendMessageAsync("Authorised " + Convert.ToUInt64(id));
                    }
                    Config.Instance.Write();
                }
                catch (FormatException)
                {
                    await message.Channel.SendMessageAsync("Invalid id");
                }
            }
        }

        /// <summary>
        /// If the user is authorised, adds another channel to be handled by the bot to the config and start handler for it
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task AddChannel(SocketMessage message)
        {
            if (!Config.Instance.AuthUsersIds.Contains(message.Author.Id)) return;
            string id = null;
            if ((message.Content.IndexOf(' ') != -1) && message.Content.Length > "!addchannel 1".Length) id = message.Content.Substring(message.Content.IndexOf(' ') + 1);
            if (id == null)
            {
                await message.Channel.SendMessageAsync("Invalid id");
            }
            else
            {
                ulong channelId = 0;
                try
                {
                    channelId = Convert.ToUInt64(id);
                }
                catch (FormatException)
                {
                    await message.Channel.SendMessageAsync("Invalid id");
                }
                var channelconfig = new ChannelConfig()
                {
                    ChannelId = channelId
                };
                Config.Instance.ChannelConfigs.Add(channelconfig);
                Config.Instance.Write();
                await Bot.StartChannelHandler(channelconfig);
            }
        }
    }
}
