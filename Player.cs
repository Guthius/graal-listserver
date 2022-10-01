using System.Net.Sockets;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Listserver.Database;
using Serilog;

namespace Listserver;

public class Player
{
    private const int MsgServerlist = 0;
    private const int MsgMotd = 2;
    private const int MsgShowmore = 3;
    private const int MsgDisconnect = 4;
    private const int MsgPaybycreditcard = 5;
    private const int MsgPaybyphone = 6;

    private string _outgoingData = string.Empty;
    private readonly Socket _socket;
    private readonly byte[] _receiveBuffer = new byte[4096];
    private readonly byte[] _inflateBuffer = new byte[204800];
    private readonly ServerSettings _settings;
    private readonly IDatabase _database;

    /// <summary>
    /// Gets the IP address of the remote client.
    /// </summary>
    private string Ip { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="socket">The client socket.</param>
    /// <param name="database">The database.</param>
    /// <param name="settings">The server settings.</param>
    public Player(Socket socket, IDatabase database, ServerSettings settings)
    {
        _settings = settings;
        _database = database;
        _socket = socket;

        Ip = socket.RemoteEndPoint?.ToString() ?? string.Empty;

        Log.Information("'{ClientIp}' has connected", Ip);
    }

    /// <summary>
    /// Compress data so it can be send to the client.
    /// </summary>
    /// <param name="data">Data to compress.</param>
    /// <returns>Array containg the uncompressed bytes.</returns>
    private static byte[] Compress(string data)
    {
        var memoryStream = new MemoryStream();

        using var deflaterStream =
            new DeflaterOutputStream(memoryStream, new Deflater(Deflater.BEST_COMPRESSION, false));

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
        if (_outgoingData != string.Empty)
        {
            Send(_outgoingData);

            _outgoingData = string.Empty;
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
        packet[0] = (byte) ((packetData.Length >> 8) & 0xFF);
        packet[1] = (byte) (packetData.Length & 0xFF);

        /* Move the data of the bData to the bPacket array right behind the packet size. */
        for (var i = 0; i < packetData.Length; i++) packet[i + 2] = packetData[i];

        /* Send the data to the client. */
        try
        {
            _socket.Send(packet);

            Log.Debug("Sent data to '{ClientIp}' ({Len} bytes)", Ip, packet.Length);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send data to '{ClientIp}' ({Len} bytes)", Ip, packet.Length);
        }
    }

    /// <summary>
    /// Creates a properly formatted packet and adds it to the outgoing messages.
    /// </summary>
    /// <param name="type">The type of this packet.</param>
    /// <param name="data">The actual data of the packet.</param>
    private void Send(int type, string data) =>
        _outgoingData += Convert.ToString((char) (type + 32)) + data + Convert.ToString((char) 10);

    /// <summary>
    /// Disconnects the player from the server.
    /// </summary>
    /// <param name="message">The message that is shown to the player.</param>
    private void Disconnect(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Send(Convert.ToString((char) (32 + MsgDisconnect)) + message + Convert.ToString((char) 10));
        }

        _socket.Close();
    }

    /// <summary>
    /// Changes the message shown in the bottom center area of the client.
    /// </summary>
    /// <param name="message">The message to display in the bottom.</param>
    private void ShowMessage(string message) => Send(MsgMotd, message);

    /// <summary>
    /// Sends the serverlist the the player.
    /// </summary>
    private void SendServerList() => Send(MsgServerlist, _database.GetServers());

    /// <summary>
    /// Makes the 'Show More' button visible on the client, which will lead them to the specified URL when clicked.
    /// </summary>
    /// <param name="url">The URL the button should lead to.</param>
    private void EnableShowMore(string url) => Send(MsgShowmore, url);

    /// <summary>
    /// Makes the 'Pay by Credit Card' button visible on the client, which will lead them to the specified URL when clicked.
    /// </summary>
    /// <param name="url">The URL the button should lead to.</param>
    private void EnablePayByCreditCard(string url) => Send(MsgPaybycreditcard, url);

    /// <summary>
    /// Makes the 'Pay by Phone' button visible on the client.
    /// </summary>
    private void EnablePayByPhone() => Send(MsgPaybyphone, "1");

    /// <summary>
    /// Runs the player logic.
    /// </summary>
    public async Task Run(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var bytesReceived = await _socket.ReceiveAsync(_receiveBuffer.AsMemory(), SocketFlags.None, stoppingToken);
            if (bytesReceived == 0)
            {
                break;
            }

            Log.Debug("Received data from '{ClientIp}' ({Len} bytes)", Ip, bytesReceived);

            var packetSize = (_receiveBuffer[0] << 8) | _receiveBuffer[1];
            if (bytesReceived < (packetSize + 2))
            {
                continue;
            }

            var inflater = new Inflater();

            inflater.SetInput(_receiveBuffer, 2, packetSize);
            inflater.Inflate(_inflateBuffer, 0, 204800);

            /* Split the received data into packages, and handle them. */
            var messages = Encoding.UTF8.GetString(_inflateBuffer, 0, (int) inflater.TotalOut).Split((char) 10);
            foreach (var message in messages)
            {
                if (message != string.Empty)
                {
                    Handle(message[0] - 32, message[1..]);
                }
            }
        }

        Log.Information("'{ClientIp}' has disconnected", Ip);
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
                var offset = 0;
                var len = messageData[offset] - 32;
                var accountName = messageData.Substring(1, len);
                offset += len + 1;
                len = messageData[offset] - 32;
                var password = messageData.Substring(offset + 1, len);

                /* Check if the username and password are valid. */
                if (_settings.DisableLogin || _database.AccountExists(accountName, password))
                {
                    if (!_settings.DisableLogin)
                    {
                        Log.Information("'{ClientIp}' has logged in as '{AccountName}'", Ip, accountName);
                    }

                    /* Get the message of the day. */
                    var motd = _settings.Motd.Trim();
                    if (motd.Length > 0)
                    {
                        motd = motd.Replace("%{AccountName}", accountName);

                        ShowMessage(motd);
                    }

                    /* Check if the 'Pay by Credit Card' button should be shown. */
                    if (_settings.PayByCreditCard)
                    {
                        var url = _settings.PayByCreditCardUrl.Trim();
                        if (url.Length > 0)
                        {
                            EnablePayByCreditCard(url);
                        }
                    }

                    /* Check if the 'Pay by Phone' button should be shown. */
                    if (_settings.PayByPhone) EnablePayByPhone();

                    /* Check if the 'Show More' button should be shown. */
                    if (_settings.ShowMore)
                    {
                        var url = _settings.ShowMoreUrl.Trim();
                        if (url.Length > 0)
                        {
                            EnableShowMore(url);
                        }
                    }

                    SendServerList();
                }
                else
                {
                    Log.Error("Login failed for '{ClientIp}'", Ip);

                    Disconnect("Invalid account name or password.");
                }

                break;
        }

        Flush();
    }
}