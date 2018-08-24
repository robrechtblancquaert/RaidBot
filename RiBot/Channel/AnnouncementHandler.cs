using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RiBot;

namespace RiBot.Channel
{
    class AnnouncementHandler : IMessageHandler
    {
        public CommandType MessageType { get; } = CommandType.Announcement;

        public List<string> AcceptedCommands { get; } = new List<string> { "!announce" };

        public async Task<IUserMessage> Handle(IUserMessage message, IMessage command, IMessageChannel channel)
        {
            // The message currently posted in the channel, may be changed later
            IUserMessage postedMessage = message;

            // User has to be authorised
            if (!Config.Instance.AuthUsersIds.Contains(command.Author.Id)) return postedMessage;

            string content = command.Content;

            // Check if the command structure is valid, and extract the announcement
            string announcement = null;
            if (content.Length > "!announce a".Length && content.IndexOf(' ') != -1)
            {
                announcement = "|\t" + content.Substring(content.IndexOf(' ') + 1).ToLower();
            }
            if (announcement == null) return postedMessage;

            // Post the header message
            if (message == null)
            {
                postedMessage = await channel.SendMessageAsync($"• Announcements: ");
            }
            else
            {
                try
                {
                    await message.ModifyAsync(x => x.Content = $"• Announcements: ");
                }
                catch (Exception)
                {
                    postedMessage = await channel.SendMessageAsync($"• Announcements: ");
                }
            }

            // Expiration time of the announcement
            DateTime expiresOn = DateTime.Now.AddHours(4);

            // If the commands contains a time argument, change the expiration date of the announcement accordingly
            List<Argument> arguments = Argument.InString(announcement);
            if(arguments.Count > 0)
            {
                try
                {
                    string time = arguments.Where(x => x.Key == "time").SingleOrDefault().Value;
                    TimeSpan timeSpan = TimeSpan.ParseExact(time, "hh\\:mm", CultureInfo.InvariantCulture);
                    expiresOn = DateTime.Now.Add(timeSpan);
                    announcement = announcement.Remove(announcement.IndexOf('['), arguments[0].Raw.Length);
                } catch (Exception) { }
            }

            // Post the announcement and add it to the config, with an expiration date
            var postedAnnouncement = channel.SendMessageAsync(announcement).Result;
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == channel.Id).Single().Announcements.Add((ulong)postedAnnouncement.Id, expiresOn);
            Config.Instance.Write();

            return postedMessage;
        }

        public string DefaultMessage()
        {
            return "• Announcements: ";
        }
    }
}
