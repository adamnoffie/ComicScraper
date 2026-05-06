using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Util
{
    /// <summary>
    /// This class is mostly here to make converting this to a non-Console app, if desired, easier. 
    /// I figured Interface classes and Dependency Injection might be overkill for the script.
    /// </summary>
    static class Logger
    {
        public static string LogFilePath { get; set; }

        public static string SessionLog { get { return _sessionLog.ToString(); } }
        private static readonly StringBuilder _sessionLog = new();

        /// <summary>
        /// Write a message to the console and log file (if path to log file is set)
        /// </summary>
        /// <param name="messageFormat">The message format string.</param>
        /// <param name="args">The arguments to format into the message string, if any.</param>
        public static void WriteLine(string messageFormat, params object[] args)
        {
            string msg;
            if (args == null || args.Length == 0)
                msg = messageFormat;
            else
                msg = string.Format(messageFormat, args);
            
            Console.WriteLine(msg);
            WriteToLogFile(msg);
        }
        
        // Write to the log file if path to log file is set
        private static void WriteToLogFile(object message)
        {            
            if (!string.IsNullOrEmpty(LogFilePath))
            {
                string msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " " + message.ToString() + Environment.NewLine;
                File.AppendAllText(LogFilePath, msg, Encoding.ASCII);
                _sessionLog.Append(msg);
            }
        }
    }
}
