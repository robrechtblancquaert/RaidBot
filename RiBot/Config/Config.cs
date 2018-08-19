using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiBot
{
    /// <summary>
    /// Config class, represent a config.json file
    /// </summary>
    public class Config
    {
        // The ids of all the users authorised to make admin commands
        public List<ulong> AuthUsersIds { get; set; }
        // A list of all the channels the bot handles
        public List<ChannelConfig> ChannelConfigs { get; set; } = new List<ChannelConfig>();
        // Key for the test bot client
        public string TestBotKey;
        // Key for the release bot client
        public string ReleaseBotKey;

        // Singleton
        static Config() { }
        private Config() { }
        private static readonly Config instance = new Config();
        public static Config Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Write the current class to it's file
        /// </summary>
        public void Write()
        {
            string json = JsonConvert.SerializeObject(this);
            string path = "config.json";
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Update the current class from it's file
        /// </summary>
        public void Read()
        {
            try
            {
                string path = "config.json";
                string json = System.IO.File.ReadAllText(path);
                Config config = JsonConvert.DeserializeObject<Config>(json);
                this.AuthUsersIds = config.AuthUsersIds ?? new List<ulong>();
                this.ChannelConfigs = config.ChannelConfigs ?? new List<ChannelConfig>();
                this.TestBotKey = config.TestBotKey;
                this.ReleaseBotKey = config.ReleaseBotKey;
            }
            catch (Exception) {
                Writer.Log("Could not read config file, exiting");
                System.Environment.Exit(1);
            }

        }
    }
}
