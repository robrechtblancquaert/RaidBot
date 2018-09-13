﻿using RiBot.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiBot
{
    public class ChannelData
    {
        // The messages that have been posted in the channel, represented by which type they are and there id
        public Dictionary<CommandType, ulong> PostedMessages { get; set; } = new Dictionary<CommandType, ulong>();
        // The announcements that have been posted in the channel, represented by there id and the datetime of when they should expire
        public Dictionary<ulong, DateTime> Announcements { get; set; } = new Dictionary<ulong, DateTime>();
        // All the people who are attending, represented by there username and the class the chose
        public Dictionary<string, Class> Usernames { get; set; } = new Dictionary<string, Class>();
        // The currently used roster
        public Dictionary<Class, int> Roster { get; set; } = new Dictionary<Class, int>();
        // The currently used schedule
        public Dictionary<DayOfWeek, TimeSpan> Schedule = new Dictionary<DayOfWeek, TimeSpan>();
    }
}
