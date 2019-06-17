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
    public class Player
    {
        public string Username;
        public Socket Sock;
        private string Outgoing = "";

        /// <summary>
        /// Compress data so it can be send to the client.
        /// </summary>
        /// <param name="Data">Data to compress.</param>
        /// <returns>Array containg the uncompressed bytes.</returns>
        public byte[] Compress(string Data)
        {
            /* Compress data that has to be send. */
            MemoryStream MemStream = new MemoryStream();
            DeflaterOutputStream DefStream = new DeflaterOutputStream(MemStream, new Deflater(Deflater.BEST_COMPRESSION, false));
            byte[] Buffer = Encoding.ASCII.GetBytes(Data);
            DefStream.Write(Buffer, 0, Buffer.Length);
            DefStream.Finish();
            DefStream.Close();
            return MemStream.ToArray();
        }

        /// <summary>
        /// Sends all data that is waiting to be send, and clears the
        /// outgoing messages.
        /// </summary>
        public void SendOutgoing()
        {
            if (Outgoing != String.Empty)
            {
                Send(Outgoing);

                Outgoing = String.Empty;
            }
        }

        /// <summary>
        /// Directly sends the specified data to the player.
        /// </summary>
        /// <param name="Data">The complete package to send.</param>
        public void Send(string Data)
        {
            byte[] bData = Compress(Data);
            byte[] bPacket = new byte[bData.Length + 2];

            // Set the size of this packet.
            bPacket[0] = (byte)((bData.Length >> 8) & 0xFF);
            bPacket[1] = (byte)(bData.Length & 0xFF);

            // Move the data of the bData to the bPacket
            // array right behind the packet size.
            for (int i = 0; i < bData.Length; i++) bPacket[i + 2] = bData[i];

            // Send the data to the client.
            try
            {
                Sock.Send(bPacket);
                Log.ToConsole("Server", "Send data to " + Sock.RemoteEndPoint + " (" + bPacket.Length.ToString() + " bytes)", 10);
            }
            catch (Exception e)
            {
                /* Display a stack trace of the error. */
                Log.ToConsole("Server", e.Message, 4);
                Log.ToConsole("Server", e.StackTrace, 4);
                Log.ToConsole("Server", "Failed to send data to " + Sock.RemoteEndPoint + " (" + bPacket.Length.ToString() + " bytes)", 12);
            }
        }

        /// <summary>
        /// Creates a properly formatted package and adds it to
        /// the outgoing messages.
        /// </summary>
        /// <param name="Type">The type of this package.</param>
        /// <param name="Data">The actual data of the package.</param>
        public void Send(int Type, string Data)
        {
            /* Generate the message to send. */
            string sMessage = Convert.ToString((char)(Type + 32)) + Data + Convert.ToString((char)10);
            Outgoing += sMessage;
        }

        /// <summary>
        /// Disconnects the players from the server.
        /// </summary>
        /// <param name="Message">The message that is shown to the player.</param>
        private void Disconnect(string Message)
        {
            string sMessage = Convert.ToString((char)(36)) + Message + Convert.ToString((char)10);
            Send(sMessage);

            //Send(4, Message);
            Sock.Close();
        }

        /// <summary>
        /// Changes the message shown in the bottom center area of the client.
        /// </summary>
        /// <param name="Message">The message to display in the bottom.</param>
        public void ShowMessage(string Message)
        {
            Send(2, Message);
        }

        /// <summary>
        /// Sends the serverlist the the player.
        /// </summary>
        public void SendServerList()
        {
            Send(0, Program.DB.GetServers());
        }

        /// <summary>
        /// Makes the 'Show More' button visible on the client, which
        /// will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="URL">The URL the button should lead to.</param>
        public void EnableShowMore(string URL)
        {
            Send(3, URL);
        }

        /// <summary>
        /// Makes the 'Pay by Credit Card' button visible on the client, which
        /// will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="URL">The URL the button should lead to.</param>
        public void EnablePayByCreditCard(string URL)
        {
            Send(5, URL);
        }

        /// <summary>
        /// Makes the 'Pay by Phone' button visible on the client, which
        /// will lead them to the specified URL when clicked.
        /// </summary>
        public void EnablePayByPhone()
        {
            Send(6, "1");
        }

        /// <summary>
        /// Handles any received packages.
        /// </summary>
        /// <param name="Type">Type of package.</param>
        /// <param name="Data">Data of the package.</param>
        public void Handle(int Type, string Data)
        {
            int iOffset = 0;
            int iSize = 0;
            string sURL;
            string sWelcome = "";

            switch (Type)
            {
                case 0: /* IDENTIFICATION */
                    if (Data != "newmain")
                    {
                        Disconnect("You are using a unsupported client.");
                    }
                break;

                case 1: /* LOGIN */

                    /* Extract the username and password. */
                    iSize = (int)Data[iOffset] - 32;
                    string sUsername = Data.Substring(iOffset + 1, iSize);
                    iOffset += (iSize + 1);
                    iSize = (int)Data[iOffset] - 32;
                    string sPassword = Data.Substring(iOffset + 1, iSize);

                    /* Check if the username and password are valid. */
                    if (Program.DB.AccountExists(sUsername, sPassword) || Program.DisableLogin)
                    {
                        /* Login success. */
                        if (!Program.DisableLogin)
                            Log.ToConsole("Server", Sock.RemoteEndPoint + " has logged in as " + sUsername + ".", 10);

                        /* Check if the message in the bottom of the client should
                         * be shown or not, and get the message which should be shown. */
                        if (Program.Config.Contains("welcome"))
                        {
                            sWelcome = Program.Config["welcome"];
                            if (sWelcome == String.Empty)
                            {
                                sWelcome = "Listserver 2.1.5 Emulator By Seipheroth";
                            }
                        }
                        if (Program.Config.Contains("hidewelcome"))
                        {
                            if (!Program.Config.GetBool("hidewelcome"))
                            {
                                ShowMessage(sWelcome);
                            }
                        }
                        else
                        {
                            ShowMessage(sWelcome);
                        }

                        /* Check if the 'Pay by Credit Card' button should be shown. */
                        if (Program.Config.Contains("paybycreditcard"))
                        {
                            if (Program.Config.GetBool("paybycreditcard"))
                            {
                                sURL = Program.Config["paybycreditcard_url"];
                                if (sURL == String.Empty) sURL = "localhost/";

                                EnablePayByCreditCard(sURL);
                            }
                        }

                        /* Check if the 'Pay by Phone' button should be shown. */
                        if (Program.Config.Contains("paybyphone"))
                        {
                            if (Program.Config.GetBool("paybyphone"))
                            {
                                EnablePayByPhone();
                            }
                        }

                        /* Check if the 'Show More' button should be shown. */
                        if (Program.Config.Contains("showmore"))
                        {
                            if (Program.Config.GetBool("showmore"))
                            {
                                sURL = Program.Config["showmore_url"];
                                if (sURL == String.Empty) sURL = "www.gamemagi.com/gserver";

                                EnableShowMore(sURL);
                            }
                        }

                        SendServerList();
                    }
                    else
                    {
                        /* Login failed. */
                        Log.ToConsole("Server", "Login failed for " + Sock.RemoteEndPoint+ ".", 12);
                        Disconnect("Invalid username or password.");
                    }
                break;
            }
        }
    }
}
