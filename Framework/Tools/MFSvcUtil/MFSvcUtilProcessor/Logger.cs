using System;
using System.Collections.Generic;
using System.Text;

namespace Ws.SvcUtilCodeGen
{
    public enum LogLevel
    {
        Normal,
        Verbose,
    }

    public static class Logger
    {
        public static bool Verbose = false;

        public static void WriteLine(string text, LogLevel logLevel)
        {
            if (logLevel == LogLevel.Normal)
                Console.WriteLine(text);
            else if (logLevel == LogLevel.Verbose && Verbose == true)
                Console.WriteLine(text);
        }
    }
}
