using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Listserver
{
    public class Player
    {
        const int MSG_SERVERLIST = 0;
        const int MSG_MOTD = 2;
        const int MSG_SHOWMORE = 3;
        const int MSG_DISCONNECT = 4;
        const int MSG_PAYBYCREDITCARD = 5;
        const int MSG_PAYBYPHONE = 6;

        string outgoingData = "";
        readonly Socket socket;
        readonly byte[] receiveBuffer = new byte[4096];
        readonly byte[] inflateBuffer = new byte[204800];

        /// <summary>
        /// Gets or sets the ID of the player.
        /// </summary>
        public string ID { get; private set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="socket"></param>
        public Player(Socket socket)
        {
            ID = socket.RemoteEndPoint.ToString();

            this.socket = socket;
            this.socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, this.OnDataReceived, null);
        }

        /// <summary>
        /// Compress data so it can be send to the client.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>Array containg the uncompressed bytes.</returns>
        byte[] Compress(string data)
        {
            var memoryStream = new MemoryStream();

            using (var deflaterStream = new DeflaterOutputStream(memoryStream, new Deflater(Deflater.BEST_COMPRESSION, false)))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                deflaterStream.Write(buffer, 0, buffer.Length);
                deflaterStream.Finish();

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Sends all data that is waiting to be send, and clears the outgoing messages.
        /// </summary>
        void Flush()
        {
            if (outgoingData != string.Empty)
            {
                Send(outgoingData);

                outgoingData = string.Empty;
            }
        }

        /// <summary>
        /// Sends the specified data directly to the player.
        /// </summary>
        /// <param name="data">The packet to send.</param>
        void Send(string data)
        {
            var packetData = Compress(data);
            var packet = new byte[packetData.Length + 2];

            // Set the size of this packet.
            packet[0] = (byte)((packetData.Length >> 8) & 0xFF);
            packet[1] = (byte)(packetData.Length & 0xFF);

            // Move the data of the bData to the bPacket array right behind the packet size.
            for (int i = 0; i < packetData.Length; i++) packet[i + 2] = packetData[i];

            // Send the data to the client.
            try
            {
                socket.Send(packet);
                Log.Write(LogLevel.Debug, "Player", "Sent data to {0} ({1} bytes)", ID, packet.Length);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "Player", "Failed to send data to {0} ({1} bytes)", ID, packet.Length);
                Log.Write(LogLevel.Error, "Player", ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Creates a properly formatted packet and adds it to the outgoing messages.
        /// </summary>
        /// <param name="type">The type of this packet.</param>
        /// <param name="data">The actual data of the packet.</param>
        void Send(int type, string data) => outgoingData += Convert.ToString((char)(type + 32)) + data + Convert.ToString((char)10);

        /// <summary>
        /// Disconnects the player from the server.
        /// </summary>
        /// <param name="message">The message that is shown to the player.</param>
        void Disconnect(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Send(Convert.ToString((char)(32 + MSG_DISCONNECT)) + message + Convert.ToString((char)10));
            }

            socket.Close();
        }

        /// <summary>
        /// Changes the message shown in the bottom center area of the client.
        /// </summary>
        /// <param name="message">The message to display in the bottom.</param>
        void ShowMessage(string message) => Send(MSG_MOTD, message);

        /// <summary>
        /// Sends the serverlist the the player.
        /// </summary>
        void SendServerList() => Send(MSG_SERVERLIST, Program.Database.GetServers());

        /// <summary>
        /// Makes the 'Show More' button visible on the client, which will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="url">The URL the button should lead to.</param>
        void EnableShowMore(string url) => Send(MSG_SHOWMORE, url);

        /// <summary>
        /// Makes the 'Pay by Credit Card' button visible on the client, which will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="url">The URL the button should lead to.</param>
        void EnablePayByCreditCard(string url) => Send(MSG_PAYBYCREDITCARD, url);

        /// <summary>
        /// Makes the 'Pay by Phone' button visible on the client.
        /// </summary>
        void EnablePayByPhone() => Send(MSG_PAYBYPHONE, "1");

        /// <summary>
        /// Called whenever data is received from the player.
        /// </summary>
        /// <param name="asyncResult"></param>
        void OnDataReceived(IAsyncResult asyncResult)
        {
            var bytesReceived = socket.EndReceive(asyncResult);
            if (bytesReceived > 0)
            {
                int packetSize = (receiveBuffer[0] >> 8) + receiveBuffer[1];

                Log.Write(LogLevel.Debug, "Player", "Received data from {0} ({1} bytes)", ID, packetSize + 2);

                if (bytesReceived >= (packetSize + 2))
                {
                    var inflater = new Inflater();
                    inflater.SetInput(receiveBuffer, 2, packetSize);
                    inflater.Inflate(inflateBuffer, 0, 204800);

                    // Split the received data into packages, and handle them.
                    string[] messages = Encoding.UTF8.GetString(inflateBuffer, 0, (int)inflater.TotalOut).Split((char)10);
                    for (int j = 0; j < messages.Length; j++)
                    {
                        if (messages[j] != string.Empty)
                        {
                            Handle(messages[j][0] - 32, messages[j].Substring(1));
                        }
                    }
                }

                socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, OnDataReceived, null);
            }
            else
            {
                Log.Write(LogLevel.Info, "Player", "{0} has disconnected", ID);

                return;
            }
        }

        /// <summary>
        /// Handles messages received from the client.
        /// </summary>
        /// <param name="messageType">Type of package.</param>
        /// <param name="messageData">Data of the package.</param>
        void Handle(int messageType, string messageData)
        {
            switch (messageType)
            {
                /* IDENTIFICATION */
                case 0:
                    if (messageData != "newmain")
                    {
                        Disconnect("You are using a unsupported client.");
                    }
                    break;

                /* LOGIN */
                case 1:

                    // Extract the account name and password.
                    int offset = 0;
                    int len = messageData[offset] - 32;
                    var accountName = messageData.Substring(offset + 1, len);
                    offset += len + 1;
                    len = messageData[offset] - 32;
                    var password = messageData.Substring(offset + 1, len);

                    // Check if the username and password are valid.
                    if (Program.LoginDisabled || Program.Database.AccountExists(accountName, password))
                    {
                        // Login success.
                        if (!Program.LoginDisabled)
                            Log.Write(LogLevel.Info, "Player", "{0} has logged in as {1}", ID, accountName);

                        // Append the account name to the ID.
                        ID = ID + " (" + accountName + ")";

                        // Get the message of the day.
                        var motd = Program.Configuration.Get("motd", "").Trim();
                        if (motd.Length > 0)
                        {
                            ShowMessage(motd);
                        }

                        // Check if the 'Pay by Credit Card' button should be shown.
                        if (Program.Configuration.GetBool("paybycreditcard", false))
                        {
                            var url = Program.Configuration.Get("paybycreditcard_url", "").Trim();
                            if (url.Length > 0)
                            {
                                EnablePayByCreditCard(url);
                            }
                        }

                        // Check if the 'Pay by Phone' button should be shown.
                        if (Program.Configuration.GetBool("paybyphone", false)) EnablePayByPhone();

                        // Check if the 'Show More' button should be shown.
                        if (Program.Configuration.GetBool("showmore", false))
                        {
                            var url = Program.Configuration.Get("showmore_url", "").Trim();
                            if (url.Length > 0)
                            {
                                EnableShowMore(url);
                            }
                        }

                        SendServerList();
                    }
                    else
                    {
                        Log.Write(LogLevel.Error, "Player", "Login failed for {0}", ID);

                        Disconnect("Invalid account name or password.");
                    }
                    break;
            }

            Flush();
        }
    }
}