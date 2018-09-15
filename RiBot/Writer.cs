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
        // The logfile currenty used
        public static string LogFile { get; set; }

        /// <summary>
        /// Create a log file, and a logs directory if it doesn't exist
        /// </summary>
        public static void Initialise()
        {
            System.IO.Directory.CreateDirectory("logs");
            LogFile = "log-" + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("nl-BE"));
    }

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
            string toLog = $"[{DateTime.Now.ToLocalTime().ToString("G", CultureInfo.CreateSpecificCulture("nl-BE"))} | RiBot] " + s;
            Console.WriteLine(toLog);

            System.IO.File.AppendAllText("logs/" + LogFile, toLog + "\n");
        }
    }
}
