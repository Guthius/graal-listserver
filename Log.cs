using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Listserver
{
    // COLOR CODES:
    //
    //  1 = dark blue
    //  2 = dark green
    //  4 = dark red
    //  5 = purple
    //  6 = gold
    //  7 = light gray (default)
    //  8 = gray
    //  9 = blue
    // 10 = green
    // 11 = light blue (teal)
    // 12 = red
    // 13 = pink
    // 14 = yellow
    // 15 = white

    public class Log
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);

        /// <summary>
        /// Get the handle for the console window.
        /// </summary>
        public static IntPtr ConsoleHandle = GetStdHandle(0xfffffff5);

        /// <summary>
        /// Write a line of information into the console.
        /// </summary>
        /// <param name="Prefix">The prefix of the message, this is usually something like Error, Notice, etc.</param>
        /// <param name="Value">This is the actual information or message that should be shown.</param>
        /// <param name="cid">Id of the color to use for the prefix.</param>
        public static void ToConsole(string Prefix, string Message, int cid)
        {
            SetConsoleTextAttribute(ConsoleHandle, cid);
            Console.Write("[" + Prefix + "]");

            SetConsoleTextAttribute(ConsoleHandle, 15);
            Console.Write(": " + Message + "\n");
        }

        /// <summary>
        /// Write a new log entry to the specified file.
        /// </summary>
        /// <param name="Filename">Location of the log file.</param>
        /// <param name="Message">Message to write to the log file.</param>
        /// <param name="cid">Color used to write the message to the console.</param>
        public static void Write(string Filename, string Message, int cid)
        {
            FileStream LogFile = new FileStream(Filename, FileMode.Append);
            StreamWriter Output = new StreamWriter(LogFile);
            try
            {
                /* Write the message to the log file and the console. */
                Output.WriteLine(DateTime.Now.ToString() + " - " + Message);
                SetConsoleTextAttribute(ConsoleHandle, cid);
                Console.WriteLine(Message);
                SetConsoleTextAttribute(ConsoleHandle, 7);
            }
            finally
            {
                Output.Close();
                LogFile.Close();
            }
        }

        /// <summary>
        /// Write a new log entry to the specified file.
        /// </summary>
        /// <param name="Filename">Location of the log file.</param>
        /// <param name="Message">Message to write to the log file.</param>
        public static void Write(string Filename, string Message)
        {
            Write(Filename, Message, 15);
        }
    }
}
