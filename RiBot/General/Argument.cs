using System;
using System.Collections.Generic;
using System.Text;

namespace RiBot
{
    /// <summary>
    /// A class to easily process arguments given in messages to the bot, a long as they are in the correct format
    /// </summary>
    public class Argument
    {
        public string Raw { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Creates an Argument from a string with the following format: [key:value]
        /// </summary>
        /// <param name="s">An argument in the correct format</param>
        public Argument(string s)
        {
            if (s == null) throw new ArgumentException();
            if (!(s[0] == '[')) throw new ArgumentException();
            if (!(s.Contains(":"))) throw new ArgumentException();
            if (!(s.IndexOf(']') == s.Length - 1)) throw new ArgumentException();
            this.Raw = s;

            int middleIndex = s.IndexOf(':');
            this.Key = s.Substring(1, middleIndex - 1);
            this.Value = s.Substring(middleIndex + 1, s.Length - this.Key.Length - 3);
        }

        /// <summary>
        /// Create a list of all the arguments that are present in a string
        /// </summary>
        /// <param name="s">String that contains arguments</param>
        /// <returns>List of arguments in the string</returns>
        public static List<Argument> InString(string s)
        {
            List<Argument> arguments = new List<Argument>();

            int startIndex = s.IndexOf('[');
            int endIndex = s.IndexOf(']');

            while(startIndex != -1 && endIndex != -1)
            {
                try
                {
                    Argument argument = new Argument(s.Substring(startIndex, endIndex - startIndex + 1));
                    arguments.Add(argument);
                }
                catch (ArgumentException) { }

                s = s.Remove(0, s.IndexOf(']') + 1);
                startIndex = s.IndexOf('[');
                endIndex = s.IndexOf(']');
            }
            
            return arguments;
        }
    }
}
