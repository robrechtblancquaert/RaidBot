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
        public async Task Handle(Command command)
        {
            if (command.Author.IsBot) return;
            if (command.Channel.GetType() != typeof(SocketDMChannel)) return;
            switch (command.FirstWord)
            {
                /*case "!help":
                    await SendHelp(command);
                    break;
                case "!config":
                    await SendConfig(command);
                    break;
                case "!auth":
                    await AuthId(command);
                    break;*/
                case "!addchannel":
                    await AddChannel(command);
                    break;
                default:
                    await command.Channel.SendMessageAsync("Uh oh! I can't do that! Did you mean to send that command in a channel?");
                    break;
            }
        }

        /*
        /// <summary>
        /// Sends a list of commands to the person sending the message, more commands if that person is an authorised user
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task SendHelp(SocketMessage message)
        {
            // A couple of giant strings that contain all the info of the possible commands (discord has max message size, so split them up)
            string help = @"**HELP FOR RAIDBOT**
__List of commands in DM:__
-   | `!help` => Displays this message.

__List of commands in channel:__
-   | `x {class}` => Register that you are attending the next raid. Replace `{class}` with an identifiable classname.
     This means you only need to type as many letters of the class name as to avoid there being any other class names that start with the same letters.
```x guardian
x g
x r => Will not work because both 'ranger' and 'revenant' start with r```     Possible values for class: `[Guardian, Revenant, Warrior, Engineer, Ranger, Thief, Elementalist, Mesmer, Necromancer, None]`
     If an invalid class or no class is given, you will be registered under `None`
-   | `!x` => Remove yourself from the attendance list for next raid.
";
            await message.Channel.SendMessageAsync(help);
            if (Config.Instance.AuthUsersIds.Contains(message.Author.Id))
            {
                string adminhelp = @"°º¤ø,¸¸,ø¤º°\`°º¤ø,¸,ø¤°º¤ø,¸¸,ø¤º°\`°º¤ø,¸

**ADMIN HELP**
__Commands for admins in DM:__
-   | `!config` => Returns a json of the current bot configuration.
-   | `!auth {userid}` => Makes a user an admin. `{userid}` is the id of the user you want to make an admin. Get the id by right-clicking on a user with developer mode enabled.
-   | `!addchannel {channelid}` => Tell the bot to take over a channel, where `{channelid}` is that channel's id. The id is obtained by right clicking on the text-channel when developer mode is enabled.
     WARNING: THIS WILL DESTROY ANY MESSAGES IN THAT CHANNEL!

__Commands for admins in channel:__
-   | `!border {color}` => Changes the border message to display the given color.
-    Possible values for {color}: `[Blue, Red, Green, OS, Eotm]` (capital letters not required)
-   | `!reset` => Reset attendance and border colour. This command will automatically be called every night at 3.
-   | `!welcome {message}` => Replaces the welcome message by `{message}`
-   | `!announce {announcement} {time}` => Places the `{announcement}` in the channel.
Optionally you can add `{time}` anywhere in the message in the format of `[time:hh:mm]` to make it expire after a given time.
```!announce something [time:00:05] => Will expire after 5 minutes.
!announce [time:99:30] something one -h 99:30 => Will expire after 99 hours and 30 minutes.
!announce something that is two => Will not expire.```
";
                await message.Channel.SendMessageAsync(adminhelp);

                string adminhelp_2 = @"-   | `!roster {values}` => Assigns a specific roster to attendance, {values} consists of multiple assignements formed as such: `[{class}:{number}]`.
     Class is assigned the same way as in the `x {class}` command, so you don't have to type the whole class name.
```!roster [g:3][war:3][mes:1]```-   | `!schedule {values}` => Sets the default weekly schedule. The strucure of `{values}` is as follows: `[{day}:{hh:mm}]`. Where `{day}` is a day of the week (full name not required).
`{hh:mm}` is the time of the raid, same as in the `!announce` command, but it can alsoe be `reset` to remove the raidtime from the schedule.
```!schedule [mo:20:00][tue:20:00][su:18:00][wed:14:00] => sets the schedule for a week with given times.
!schedule [monday:reset] => removes monday from the schedule.
```
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
        }*/ // Move these to a channel handler

        /// <summary>
        /// If the user is authorised, adds another channel to be handled by the bot to the config and start handler for it
        /// </summary>
        /// <param name="message">The message received by the bot</param>
        private async Task AddChannel(Command command)
        {
            ulong guildId = 0;
            try
            {
                guildId = Convert.ToUInt64(command.MessageRest);
            }
            catch (FormatException)
            {
                await command.Channel.SendMessageAsync("Invalid id");
            }

            // Test if that guild already has a channel
            if (Bot.Client.GetGuild(guildId).Channels.Where(x => Config.Instance.ChannelConfigs.Where(y => y.ChannelId == x.Id).SingleOrDefault() != null).SingleOrDefault() != null)
            {
                await command.Channel.SendMessageAsync("Channel already exists in that guild");
                return;
            }

            var channel = await Bot.Client.GetGuild(guildId).CreateTextChannelAsync($"RaidBot{DateTime.Now.ToShortDateString()}");

            var channelconfig = new ChannelConfig()
            {
                ChannelId = channel.Id,
                AuthUsersIds = new List<ulong>()
                {
                    command.Author.Id
                }
            };
            Config.Instance.General.ChannelIds.Add(channel.Id);
            Config.Instance.ChannelConfigs.Add(channelconfig);
            Config.Instance.Write();
            await Bot.StartChannelHandler(null, channel);
        }
    }
}
