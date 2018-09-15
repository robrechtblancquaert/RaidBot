using System;
using System.Collections.Generic;
using System.Text;

namespace RiBot.Models
{
    public class GeneralConfig
    {
        // Key for the test bot client
        public string TestBotKey { get; set; } = "";
        // Key for the release bot client
        public string ReleaseBotKey { get; set; } = "";
        // List of the ids of all channels managed by the bot
        public List<ulong> ChannelIds { get; set; } = new List<ulong>();
    }
}
