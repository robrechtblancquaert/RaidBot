﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace RiBot.Models
{
    /// <summary>
    /// The configuration of a specific channel in discord
    /// </summary>
    public class ChannelConfig
    {
        // The id of the channel this class is the config for
        public ulong ChannelId { get; set; } = new ulong();
        // The ids of all the users authorised to make admin commands
        public List<ulong> AuthUsersIds { get; set; } = new List<ulong>();
        // All possible classes
        public List<string> ClassTypes { get; set; } = new List<string>();
        // The data of all messages in this channel
        [JsonIgnore]
        public ChannelData ChannelData { get; set; } = new ChannelData();
    }
}
