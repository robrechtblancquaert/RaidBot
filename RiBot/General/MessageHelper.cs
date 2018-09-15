using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RiBot.General
{
    /// <summary>
    /// Provides several methods often used in handlers
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Updates a message with the given value, or creates a new message if there is no posted message.
        /// </summary>
        /// <param name="postedMessage">The message to update</param>
        /// <param name="value">The new value of the message</param>
        public static async Task<IUserMessage> UpdateMessage(IUserMessage postedMessage, string value)
        {
            var channel = postedMessage.Channel;
            if (postedMessage == null)
            {
                postedMessage = await channel.SendMessageAsync(value);
            }
            else
            {
                try
                {
                    await postedMessage.ModifyAsync(x => x.Content = value);
                }
                catch (Exception)
                {
                    postedMessage = await channel.SendMessageAsync(value);
                }
            }
            return postedMessage;
        }

        /// <summary>
        /// Extract all the possible enums that start with the given value
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">String possibly at start of enum value</param>
        /// <returns>A list of enums that start with the given value</returns>
        public static List<T> PossibleValues<T>(string value)
            where T: Enum
        {
            List<T> possibilities = new List<T>();
            foreach (var en in (T[])Enum.GetValues(typeof(T)))
            {
                if (en.ToString().Length >= value.Length)
                {
                    if (en.ToString().ToLower().Substring(0, value.Length) == value)
                    {
                        possibilities.Add(en);
                    }
                }
            }
            return possibilities;
        }

        /// <summary>
        /// Extract all the possible strings out of a list that start with the given value
        /// </summary>
        /// <param name="options">List of string that must start with a given value</param>
        /// <param name="value">The value to search for</param>
        /// <returns>A list of strings that start with the given value</returns>
        public static List<string> PossibleValues(IEnumerable<string> options, string value)
        {
            List<string> possibilities = new List<string>();
            foreach(string option in options)
            {
                if(option.Length >= value.Length)
                {
                    if(option.ToLower().Substring(0, value.Length) == value)
                    {
                        possibilities.Add(option);
                    }
                }
            }
            return possibilities;
        }
    }
}
