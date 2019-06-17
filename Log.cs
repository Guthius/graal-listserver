using System;

namespace Listserver
{
    public class Log
    {
        /// <summary>
        /// Writes a line to the log.
        /// </summary>
        /// <param name="logLevel">The log level of the log message.</param>
        /// <param name="tag">The message tag, this is usually something like Error, Notice, etc.</param> 
        /// <param name="message">The log message.</param>
        public static void Write(LogLevel logLevel, string tag, string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "]");

            Console.ForegroundColor = ConsoleColor.White;
            tag = tag.Trim();
            if (tag.Length > 0)
            {
                Console.Write("[" + tag + "] ");
            }

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

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }

            Console.Write(string.Format(message, args) + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    /// <summary>
    /// Identifies the level/importance of a log message.
    /// </summary>
    public enum LogLevel
    {
        Info,
        Error,
        Warning,
        Debug
    }
}