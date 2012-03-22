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

        private static bool _firstWrite = true;

        public static void Write(object message)
        {
            Console.Write(message);
            WriteToLogFile(message);
        }

        public static void WriteLine(object message)
        {
            Console.WriteLine(message);
            WriteToLogFile(message);
        }

        public static void WriteLine(string messageFormat, params object[] args)
        {
            string msg = string.Format(messageFormat, args);
            Console.WriteLine(msg);
            WriteToLogFile(msg);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
            WriteToLogFile(string.Empty);
        }
        
        // Write to the log file if path to log file is set
        private static void WriteToLogFile(object message)
        {            
            if (!string.IsNullOrEmpty(LogFilePath))
            {
                string msg = Environment.NewLine + message.ToString();

                if (_firstWrite)
                {
                    msg = Environment.NewLine + Environment.NewLine +
                        DateTime.Now.ToString() + Environment.NewLine +
                        "================================================================" +
                        msg;

                    _firstWrite = false;
                }

                // append to Log.txt, creating the file if it doesn't exist
                File.AppendAllText(LogFilePath, msg, Encoding.ASCII);
            }
        }
    }
}
