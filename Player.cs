using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Serilog;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Listserver
{
    public class Player
    {
        private const int MSG_SERVERLIST = 0;
        private const int MSG_MOTD = 2;
        private const int MSG_SHOWMORE = 3;
        private const int MSG_DISCONNECT = 4;
        private const int MSG_PAYBYCREDITCARD = 5;
        private const int MSG_PAYBYPHONE = 6;

        private string outgoingData = "";
        private readonly Socket socket;
        private readonly byte[] receiveBuffer = new byte[4096];
        private readonly byte[] inflateBuffer = new byte[204800];
        private readonly ServerSettings settings;
        private readonly IDatabase database;

        /// <summary>
        /// Gets the IP address of the remote client.
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="socket">The client socket.</param>
        /// <param name="database">The database.</param>
        /// <param name="settings">The server settings.</param>
        public Player(Socket socket, IDatabase database, ServerSettings settings)
        {
            this.settings = settings;
            this.database = database;
            this.socket = socket;

            Ip = socket.RemoteEndPoint.ToString();

            Log.Information("'{ClientIp}' has connected.", Ip);
        }

        /// <summary>
        /// Compress data so it can be send to the client.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>Array containg the uncompressed bytes.</returns>
        private static byte[] Compress(string data)
        {
            var memoryStream = new MemoryStream();

            using var deflaterStream = new DeflaterOutputStream(memoryStream, new Deflater(Deflater.BEST_COMPRESSION, false));

            var buffer = Encoding.ASCII.GetBytes(data);

            deflaterStream.Write(buffer, 0, buffer.Length);
            deflaterStream.Finish();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Sends all data that is waiting to be send, and clears the outgoing messages.
        /// </summary>
        private void Flush()
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
        private void Send(string data)
        {
            var packetData = Compress(data);
            var packet = new byte[packetData.Length + 2];

            /* Set the size of this packet. */
            packet[0] = (byte)((packetData.Length >> 8) & 0xFF);
            packet[1] = (byte)(packetData.Length & 0xFF);

            /* Move the data of the bData to the bPacket array right behind the packet size. */
            for (int i = 0; i < packetData.Length; i++) packet[i + 2] = packetData[i];

            /* Send the data to the client. */
            try
            {
                socket.Send(packet);

                Log.Debug("Sent data to '{ClientIp}' ({Len} bytes).", Ip, packet.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send data to '{ClientIp}' ({Len} bytes).", Ip, packet.Length);
            }
        }

        /// <summary>
        /// Creates a properly formatted packet and adds it to the outgoing messages.
        /// </summary>
        /// <param name="type">The type of this packet.</param>
        /// <param name="data">The actual data of the packet.</param>
        private void Send(int type, string data) => outgoingData += Convert.ToString((char)(type + 32)) + data + Convert.ToString((char)10);

        /// <summary>
        /// Disconnects the player from the server.
        /// </summary>
        /// <param name="message">The message that is shown to the player.</param>
        private void Disconnect(string message)
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
        private void ShowMessage(string message) => Send(MSG_MOTD, message);

        /// <summary>
        /// Sends the serverlist the the player.
        /// </summary>
        private void SendServerList() => Send(MSG_SERVERLIST, database.GetServers());

        /// <summary>
        /// Makes the 'Show More' button visible on the client, which will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="url">The URL the button should lead to.</param>
        private void EnableShowMore(string url) => Send(MSG_SHOWMORE, url);

        /// <summary>
        /// Makes the 'Pay by Credit Card' button visible on the client, which will lead them to the specified URL when clicked.
        /// </summary>
        /// <param name="url">The URL the button should lead to.</param>
        private void EnablePayByCreditCard(string url) => Send(MSG_PAYBYCREDITCARD, url);

        /// <summary>
        /// Makes the 'Pay by Phone' button visible on the client.
        /// </summary>
        private void EnablePayByPhone() => Send(MSG_PAYBYPHONE, "1");

        /// <summary>
        /// Runs the player logic.
        /// </summary>
        public async Task Run(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var bytesReceived =
                    await Task.Factory.FromAsync(
                        (cb, s) => socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, cb, s),
                        socket.EndReceive,
                        null);

                if (bytesReceived == 0)
                {
                    break;
                }

                Log.Debug("Received data from '{ClientIp}' ({Len} bytes)", Ip, bytesReceived);

                var packetSize = (receiveBuffer[0] << 8) | receiveBuffer[1];
                if (bytesReceived < (packetSize + 2))
                {
                    continue;
                }

                var inflater = new Inflater();

                inflater.SetInput(receiveBuffer, 2, packetSize);
                inflater.Inflate(inflateBuffer, 0, 204800);

                /* Split the received data into packages, and handle them. */
                var messages = Encoding.UTF8.GetString(inflateBuffer, 0, (int)inflater.TotalOut).Split((char)10);
                for (int j = 0; j < messages.Length; j++)
                {
                    if (messages[j] != string.Empty)
                    {
                        Handle(messages[j][0] - 32, messages[j][1..]);
                    }
                }
            }

            Log.Information("'{ClientIp}' has disconnected.", Ip);
        }

        /// <summary>
        /// Handles messages received from the client.
        /// </summary>
        /// <param name="messageType">Type of package.</param>
        /// <param name="messageData">Data of the package.</param>
        private void Handle(int messageType, string messageData)
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

                    /* Extract the account name and password. */
                    int offset = 0;
                    int len = messageData[offset] - 32;
                    var accountName = messageData.Substring(offset + 1, len);
                    offset += len + 1;
                    len = messageData[offset] - 32;
                    var password = messageData.Substring(offset + 1, len);

                    /* Check if the username and password are valid. */
                    if (settings.DisableLogin || database.AccountExists(accountName, password))
                    {
                        if (!settings.DisableLogin)
                        {
                            Log.Information("'{ClientIp}' has logged in as '{AccountName}'.", Ip, accountName);
                        }

                        /* Get the message of the day. */
                        var motd = settings.Motd.Trim();
                        if (motd.Length > 0)
                        {
                            motd = motd.Replace("%{AccountName}", accountName);

                            ShowMessage(motd);
                        }

                        /* Check if the 'Pay by Credit Card' button should be shown. */
                        if (settings.PayByCreditCard && settings.PayByCreditCardUrl is not null)
                        {
                            var url = settings.PayByCreditCardUrl.Trim();
                            if (url.Length > 0)
                            {
                                EnablePayByCreditCard(url);
                            }
                        }

                        /* Check if the 'Pay by Phone' button should be shown. */
                        if (settings.PayByPhone) EnablePayByPhone();

                        /* Check if the 'Show More' button should be shown. */
                        if (settings.ShowMore && settings.ShowMoreUrl is not null)
                        {
                            var url = settings.ShowMoreUrl.Trim();
                            if (url.Length > 0)
                            {
                                EnableShowMore(url);
                            }
                        }

                        SendServerList();
                    }
                    else
                    {
                        Log.Error("Login failed for '{ClientIp}'.", Ip);

                        Disconnect("Invalid account name or password.");
                    }
                    break;
            }

            Flush();
        }
    }
}