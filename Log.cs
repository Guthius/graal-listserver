using System;

namespace Listserver
{
    public enum LogLevel
    {
        Info,
        Error,
        Warning,
        Success,
        Debug
    }

    public class Log
    {
        /// <summary>
        /// Writes a line to the log.
        /// </summary>
        /// <param name="logLevel">The log level of the log message.</param>
        /// <param name="prefix">The prefix of the message, this is usually something like Error, Notice, etc.</param> 
        /// <param name="message">The log message.</param>
        public static void Write(LogLevel logLevel, string prefix, string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "]");

            switch (logLevel)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
            }

            Console.Write("[" + prefix + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(string.Format(message, args) + "\n");
        }
    }
}