using System;
using System.Net;
using System.Net.Sockets;

namespace Listserver
{
    public class Server
    {
        readonly TcpListener tcpListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="port">The server port.</param>
        public Server(int port)
        {
            try
            {
                // Start the server on the specified port...
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                Log.Write(LogLevel.Info, "Server", "The server is running on port {0}", port);
                Log.Write(LogLevel.Info, "Server", "Listening for connections...");

                // Main server loop.
                while (true)
                {
                    if (tcpListener.Pending())
                    {
                        var socket = tcpListener.AcceptSocket();

                        if (socket != null)
                        {
                            var player = new Player(socket);

                            Log.Write(LogLevel.Info, "Server", "Accepted connection from {0}", player.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "Server", ex.Message + "\n" + ex.StackTrace);

                Console.WriteLine("\nPress any key to continue");
                Console.ReadKey();
            }
        }
    }
}