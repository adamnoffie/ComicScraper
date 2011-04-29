using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    /// <summary>
    /// This class is mostly here to make converting this to a non-Console app, if desired, easier. 
    /// I figured Interface classes and Dependency Injection might be overkill for the script.
    /// </summary>
    static class Logger
    {
        public static void Write(object message)
        {
            Console.Write(message);
        }

        public static void WriteLine(object message)
        {
            Console.WriteLine(message);
        }

        public static void WriteLine(string messageFormat, params object[] args)
        {
            Console.WriteLine(string.Format(messageFormat, args));
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
