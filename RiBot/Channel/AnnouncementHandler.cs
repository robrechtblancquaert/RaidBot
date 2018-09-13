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

        public async Task<IUserMessage> Handle(IUserMessage postedMessage, Command command, bool isAuthorised = false)
        {
            // User has to be authorised
            if (!isAuthorised) return postedMessage;

            // Post the header message
            postedMessage = await MessageHelper.UpdateMessage(postedMessage, "• Announcements: ");

            string announcement = command.MessageRest;

            // Return if there is no announcement message
            if(command.MessageRest.Length == 0)
            {
                return postedMessage;
            }

            // Expiration time of the announcement
            DateTime expiresOn = DateTime.Now.AddYears(100);

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
            var postedAnnouncement = command.Channel.SendMessageAsync(announcement).Result;
            Config.Instance.ChannelConfigs.Where(x => x.ChannelId == command.Channel.Id).Single().ChannelData.Announcements.Add((ulong)postedAnnouncement.Id, expiresOn);
            Config.Instance.Write();

            return postedMessage;
        }

        public string DefaultMessage()
        {
            return "• Announcements: ";
        }
    }
}
