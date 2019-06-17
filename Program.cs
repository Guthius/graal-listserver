using System;

namespace Listserver
{
    class Program
    {
        /// <summary>
        /// Gets or sets a value indicating whether logging in has been disabled.
        /// When disabled, login details are not authenticated and anyone can connect.
        /// </summary>
        public static bool LoginDisabled { get; set; } = false;

        /// <summary>
        /// Gets the accounts database.
        /// </summary>
        public static IDatabase Database { get; private set; }

        /// <summary>
        /// Gets the server configuration.
        /// </summary>
        public static Config Configuration { get; private set; }

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        static void Main(string[] _)
        {
            Console.Title = "Graal 2.1.5 List Server";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" -------------------------------------------------");
            Console.WriteLine("  Graal 2.1.5 List Server (Created by Seipheroth)");
            Console.WriteLine(" -------------------------------------------------\n");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Load the server configuration file.
            Configuration = new Config("listserver.cfg");

            LoginDisabled = Configuration.GetBool("disablelogin", false);

            // Initialize the database.
            var databaseOk = true;
            try
            {
                Database = new JsonDatabase(Configuration);

                Log.Write(LogLevel.Info, "Program", "Database initialized succesfully");
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Error, "Program", e.Message);

                databaseOk = false;
            }

            // If the database was initialized succesfully, start the server.
            if (databaseOk)
            {
                var port = Configuration.GetInt("port", 21555);
                if (port == 0)
                {
                    port = 21555;
                }

                new Server(port);
            }
            else
            {
                Console.WriteLine("\nPress any key to continue");
                Console.ReadKey();
            }
        }
    }
}