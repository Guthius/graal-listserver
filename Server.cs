using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Listserver.Databases;

namespace Listserver
{
    public class Server
    {
        private ArrayList _players;
        private TcpListener _server;

        /// <summary>
        /// Creates a new server that runs on the specified port.
        /// </summary>
        /// <param name="Port">The port the server has to run on.</param>
        public Server(int Port)
        {
            _players = new ArrayList();

            try
            {
                /* Start the server on the specified port. */
                _server = new TcpListener(IPAddress.Parse("0.0.0.0"), Port);
                _server.Start();

                Log.ToConsole("Server", "The server is running on port " + Port.ToString() + ".", 10);
                Log.ToConsole("Server", "Listening for connections...", 10);

                /* Main application loop. */
                while (true)
                {
                    /* Check if there are any pending connections. */
                    if (this._server.Pending())
                    {
                        Player o = new Player();
                        o.Sock = _server.AcceptSocket();

                        _players.Add(o);

                        Log.ToConsole("Server", "Accepted connection from " + o.Sock.RemoteEndPoint + ".", 10);
                    }

                    /* Process data from all players. */
                    for (int i = 0; i < _players.Count; i++)
                    {
                        byte[] bReceived = new byte[4096];
                        int iTotalReceived = 0;

                        Player o = (Player)_players[i];
                        o.SendOutgoing();

                        /* Look if there is any data coming from the player. */
                        try
                        {
                            if (o.Sock.Available > 0)
                            {
                                iTotalReceived = o.Sock.Receive(bReceived);
                            }
                        }
                        catch (Exception ex)
                        {
                            /* Error receiving data, disconnect player. */
                            Log.ToConsole("Server", ex.Message, 4);
                            Log.ToConsole("Server", ex.StackTrace, 4);

                            o.Sock.Close();
                            _players.RemoveAt(i);
                            continue;
                        }

                        /* Handle the data we received. */
                        if (iTotalReceived > 0)
                        {
                            int iPacketSize = ((int)bReceived[0] >> 8) + (int)bReceived[1];
                            Log.ToConsole("Server", "Received data from " + o.Sock.RemoteEndPoint + " (" + (iPacketSize + 2).ToString() + " bytes)", 10);

                            if (iTotalReceived >= (iPacketSize + 2))
                            {
                                byte[] bBuffer = new byte[204800];
                                Inflater I = new Inflater();
                                I.SetInput(bReceived, 2, iPacketSize);
                                I.Inflate(bBuffer, 0, 204800);

                                /* Split the received data into packages, and handle them. */
                                string[] sMessages = Encoding.UTF8.GetString(bBuffer, 0, I.TotalOut).Split((char)10);
                                for (int j = 0; j < sMessages.Length; j++)
                                {
                                    if (sMessages[j] != String.Empty)
                                    {
                                        char[] sMessage = sMessages[j].ToCharArray(0,1);
                                        o.Handle((int)sMessage[0] - 32, sMessages[j].Substring(1));
                                    }
                                }
                            }
                        }

                        /* Check if we are still connected with the player. */
                        if (o.Sock == null || !o.Sock.Connected)
                        {
                            o.Sock.Close();
                            _players.RemoveAt(i);
                            continue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                /* Show a stacktrace when a error occurs. */
                Console.WriteLine("Error: " + e.Message + "\n\n" + e.StackTrace);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
}
