using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RiBot
{
    /// <summary>
    /// Handles writing to console
    /// </summary>
    public abstract class Writer
    {
        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="s">The message you want to log</param>
        public static void Log(string s)
        {
            if (s == null) return;
            if (s.Length == 0) return;
            s = s[0].ToString().ToUpper() + s.Substring(1);
            s.TrimEnd();
            Console.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("G", CultureInfo.CreateSpecificCulture("nl-BE"))} | RiBot] " + s);
        }
    }
}
