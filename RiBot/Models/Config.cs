using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RiBot.General;

namespace RiBot.Models
{
    /// <summary>
    /// Config class, represent a config.json file
    /// </summary>
    public class Config
    {
        // A list of all the channels the bot handles
        public List<ChannelConfig> ChannelConfigs { get; set; } = new List<ChannelConfig>();
        // Contains the keys for the bot, and other general information that's not channel specific
        public GeneralConfig General { get; set; } = new GeneralConfig();

        // Singleton
        static Config() { }
        private Config() {
            System.IO.Directory.CreateDirectory("data");
        }
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
            // Write the general file
            string json = JsonConvert.SerializeObject(this.General);
            string path = "data/general.json";
            System.IO.File.WriteAllText(path, json);

            // Write specific files for each channelHandler
            foreach(var channelConfig in ChannelConfigs)
            {
                System.IO.Directory.CreateDirectory($"data/channel-{channelConfig.ChannelId}");

                string configJson = JsonConvert.SerializeObject(channelConfig);
                string configPath = $"data/channel-{channelConfig.ChannelId}/config.json";
                System.IO.File.WriteAllText(configPath, configJson);

                string dataJson = JsonConvert.SerializeObject(channelConfig.ChannelData);
                string dataPath = $"data/channel-{channelConfig.ChannelId}/data.json";
                System.IO.File.WriteAllText(dataPath, dataJson);
            }
        }

        /// <summary>
        /// Update the current class from it's file
        /// </summary>
        public void Read()
        {
            try
            {
                string path = "data/general.json";
                string json = System.IO.File.ReadAllText(path);
                GeneralConfig general = JsonConvert.DeserializeObject<GeneralConfig>(json);
                this.General = general;
                foreach(var id in General.ChannelIds)
                {
                    string configPath = $"data/channel-{id}/config.json";
                    string configJson = System.IO.File.ReadAllText(configPath);
                    ChannelConfig channelConfig = JsonConvert.DeserializeObject<ChannelConfig>(configJson);

                    string dataPath = $"data/channel-{id}/data.json";
                    string dataJson = System.IO.File.ReadAllText(dataPath);
                    ChannelData channelData = JsonConvert.DeserializeObject<ChannelData>(dataJson);

                    channelConfig.ChannelData = channelData;
                    this.ChannelConfigs.Add(channelConfig);
                }
            }
            catch (Exception) {
                Writer.Log("Could not read config file, exiting");
                System.Environment.Exit(1);
            }

        }

        /// <summary>
        /// Delete a channelconfig from this class and the file system
        /// </summary>
        /// <param name="channelconfig">ChannelConfig to delete</param>
        public void DeleteConfig(ChannelConfig channelconfig)
        {
            Config.Instance.General.ChannelIds.RemoveAll(x => x == channelconfig.ChannelId);
            Config.Instance.ChannelConfigs.RemoveAll(x => x.ChannelId == channelconfig.ChannelId);

            foreach(string file in System.IO.Directory.GetFiles($"data/channel-{channelconfig.ChannelId}"))
            {
                System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete($"data/channel-{channelconfig.ChannelId}");
        }

        public ChannelConfig GetChannelConfig(IMessageChannel channel)
        {
            return this.ChannelConfigs.Where(x => x.ChannelId == channel.Id).SingleOrDefault();
        }
    }
}
