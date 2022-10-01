using System.Buffers;
using System.Net.Sockets;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using OpenGraal.Net;
using OpenGraal.Server.Database;
using OpenGraal.Server.Protocols.ListServer;
using Serilog;

namespace OpenGraal.Server;

public class Session : ISession
{
    private const int BufferSize = 204800;
    
    private readonly Socket _socket;
    private readonly ISessionHandler _handler;
    private readonly ServerSettings _settings;
    private readonly IDatabase _database;
    private bool _connected;
    private readonly SocketAsyncEventArgs _receiveEvent = new();
    private readonly byte[] _receiveBuffer;
    private int _receiveBufferOffset;
    private bool _receiving;
    private readonly object _sendLock = new();
    private readonly SocketAsyncEventArgs _sendEvent = new();
    private readonly byte[] _sendBuffer;
    private readonly PacketOutputStream _sendBufferStream;
    private bool _sending;
    private readonly byte[] _flushBuffer;
    private int _flushLen;
    private int _flushOffset;
    private readonly byte[] _inflateBuffer;
    private readonly IPacketEncoding _encoding = new ZLibPacketEncoding();
    
    public string Ip { get; }
    
    public Session(ISessionHandler handler, Socket socket, IDatabase database, ServerSettings settings)
    {
        Ip = socket.RemoteEndPoint?.ToString() ?? string.Empty;
        
        _socket = socket;
        _handler = handler;
        _settings = settings;
        _database = database;
        _connected = true;
        _receiveEvent.Completed += HandleOperationCompleted;
        _receiveBuffer = ArrayPool<byte>.Shared.Rent(4096);
        _sendEvent.Completed += HandleOperationCompleted;
        _sendBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _sendBufferStream = new PacketOutputStream(_sendBuffer);
        _flushBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _inflateBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _handler.OnConnected(this);
        
        TryReceive();
    }

    public void Send(IPacket packet)
    {
        if (!_connected)
        {
            return;
        }

        lock (_sendLock)
        {
            _sendBufferStream.WriteGChar(packet.Id);
            
            packet.WriteTo(_sendBufferStream);

            _sendBufferStream.WriteByte(0x0A);
            
            if (_sending)
            {
                return;
            }

            _sending = true;

            TrySend();
        }
    }

    private void TryReceive()
    {
        if (_receiving || !_connected)
        {
            return;
        }

        var process = true;

        while (process)
        {
            process = false;

            try
            {
                _receiving = true;
                _receiveEvent.SetBuffer(
                    _receiveBuffer,
                    _receiveBufferOffset,
                    _receiveBuffer.Length - _receiveBufferOffset);

                if (!_socket.ReceiveAsync(_receiveEvent))
                {
                    process = HandleReceive(_receiveEvent);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private void TrySend()
    {
        if (!_connected)
        {
            return;
        }

        var process = true;

        while (process)
        {
            process = false;

            lock (_sendLock)
            {
                if (_flushOffset < _flushLen)
                {
                    return;
                }
                
                _flushOffset = 0;
                
                _sendBufferStream.Flush(_encoding, _flushBuffer, out _flushLen);
                
                if (_flushLen == 0)
                {
                    _sending = false;

                    return;
                }
            }

            try
            {
                _sendEvent.SetBuffer(_flushBuffer, _flushOffset, _flushLen - _flushOffset);

                if (!_socket.SendAsync(_sendEvent))
                {
                    process = HandleSend(_sendEvent);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
    
    private void ProcessPackets(int bytesReceived)
    {
        _receiveBufferOffset += bytesReceived;

        var pos = 0;

        while (true)
        {
            var bytesLeft = _receiveBufferOffset - pos;
            if (bytesLeft < 2)
            {
                break;
            }

            var len = (_receiveBuffer[pos] << 8) | _receiveBuffer[pos + 1];
            if (len + 2 > bytesLeft)
            {
                break;
            }

            pos += 2;

            var inflater = new Inflater();

            inflater.SetInput(_receiveBuffer, pos, len);
            inflater.Inflate(_inflateBuffer, 0, BufferSize);

            var messages = Encoding.UTF8.GetString(_inflateBuffer, 0, (int)inflater.TotalOut).Split((char)10);

            foreach (var message in messages)
            {
                if (message != string.Empty)
                {
                    Handle(message[0] - 32, message[1..]);
                }
            }

            pos += len;
        }

        Array.Copy(_receiveBuffer, pos, _receiveBuffer, 0, _receiveBufferOffset - pos);

        _receiveBufferOffset -= pos;
    }
    
    private void HandleError(SocketError socketError)
    {
        _handler.OnSocketError(this, socketError);

        Disconnect();
    }

    private bool HandleReceive(SocketAsyncEventArgs e)
    {
        if (!_connected)
        {
            return false;
        }

        var size = e.BytesTransferred;

        if (size > 0)
        {
            ProcessPackets(size);
        }

        _receiving = false;

        if (e.SocketError != SocketError.Success)
        {
            HandleError(e.SocketError);

            return false;
        }

        if (size > 0)
        {
            return true;
        }

        Disconnect();

        return false;
    }

    private bool HandleSend(SocketAsyncEventArgs e)
    {
        if (!_connected)
        {
            return false;
        }

        var size = e.BytesTransferred;

        if (size > 0)
        {
            _flushOffset += size;

            if (_flushOffset >= _flushLen)
            {
                _flushOffset = 0;
            }
        }

        if (e.SocketError == SocketError.Success)
        {
            return true;
        }

        HandleError(e.SocketError);

        return false;
    }
    
    private void HandleOperationCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (!_connected)
        {
            return;
        }

        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                if (HandleReceive(e))
                {
                    TryReceive();
                }

                break;

            case SocketAsyncOperation.Send:
                if (HandleSend(e))
                {
                    TrySend();
                }

                break;

            default:
                throw new ArgumentException("The last operation completed on the socket was not a receive or send");
        }
    }
    
    public void Disconnect()
    {
        if (!_connected)
        {
            return;
        }

        try
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // ignored
            }

            _socket.Close();
            _socket.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        _connected = false;
        _handler.OnDisconnected(this);
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
                    Send(new DisconnectPacket
                    {
                        Message = "You are using a unsupported client."
                    });
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

                        Send(new MotdPacket
                        {
                            Message = motd
                        });
                    }

                    /* Check if the 'Pay by Credit Card' button should be shown. */
                    if (_settings.PayByCreditCard)
                    {
                        var url = _settings.PayByCreditCardUrl.Trim();
                        if (url.Length > 0)
                        {
                            Send(new PayByCreditCardPacket
                            {
                                Url = url
                            });
                        }
                    }

                    /* Check if the 'Pay by Phone' button should be shown. */
                    if (_settings.PayByPhone) Send(new PayByPhonePacket());

                    /* Check if the 'Show More' button should be shown. */
                    if (_settings.ShowMore)
                    {
                        var url = _settings.ShowMoreUrl.Trim();
                        if (url.Length > 0)
                        {
                            Send(new ShowMorePacket
                            {
                                Url = url
                            });
                        }
                    }

                    Send(new ServerListPacket
                    {
                        Data = _database.GetServers()
                    });
                }
                else
                {
                    Log.Error("Login failed for '{ClientIp}'", Ip);

                    Send(new DisconnectPacket
                    {
                        Message = "Invalid account name or password."
                    });
                }

                break;
        }
    }
}