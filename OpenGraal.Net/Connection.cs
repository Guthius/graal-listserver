using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using OpenGraal.Net.Encoding;

namespace OpenGraal.Net;

public sealed class Connection : IConnection
{
    private const int BufferSize = 204800;
    private const byte PacketDelimiter = 0x0A;

    private readonly Socket _socket;
    private readonly IServiceEvents _events;
    private readonly IProtocol _protocol;
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
    private readonly byte[] _deflateBuffer;
    private readonly IPacketEncoding _encoding = new ZLibPacketEncoding();
    private bool _disposed;

    public int Id { get; }
    public string Address { get; }

    public Connection(int id, IServiceEvents events, IProtocol protocol, Socket socket)
    {
        Address = socket.RemoteEndPoint?.ToString() ?? string.Empty;
        Id = id;

        _events = events;
        _protocol = protocol;
        _socket = socket;
        _connected = true;
        _receiveEvent.Completed += HandleOperationCompleted;
        _receiveBuffer = ArrayPool<byte>.Shared.Rent(4096);
        _sendEvent.Completed += HandleOperationCompleted;
        _sendBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _sendBufferStream = new PacketOutputStream(_sendBuffer);
        _flushBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _inflateBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _deflateBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        _events.OnConnected(this);

        TryReceive();
    }

    public void Send(IServerPacket packet)
    {
        if (!_connected)
        {
            return;
        }

        lock (_sendLock)
        {
            packet.WriteTo(_sendBufferStream);

            _sendBufferStream.WriteByte(PacketDelimiter);

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
                if (_flushLen == 0)
                {
                    _flushOffset = 0;

                    _sendBufferStream.Flush(_encoding, _flushBuffer, out _flushLen);
                }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParsePacket(byte[] bytes, int index, int count)
    {
        _protocol.Handle(this, bytes.AsMemory(index, count));
    }

    private void ParsePackets(byte[] bytes, int count)
    {
        var p = 0;

        for (var i = 0; i < bytes.Length; ++i)
        {
            if (bytes[i] != PacketDelimiter)
            {
                continue;
            }

            ParsePacket(bytes, p, i - p);

            p = i + 1;
        }

        var c = count - p;
        if (c > 0)
        {
            ParsePacket(bytes, p, c);
        }
    }

    private void ProcessPackets(int bytesReceived)
    {
        _receiveBufferOffset += bytesReceived;

        if (_receiveBufferOffset < 2)
        {
            return;
        }

        var bytesRead = _encoding.Decode(_receiveBuffer, 0, _receiveBufferOffset, _deflateBuffer, out var packetLen);
        if (bytesRead == 0)
        {
            return;
        }

        ParsePackets(_deflateBuffer, packetLen);

        Array.Copy(_receiveBuffer, bytesRead, _receiveBuffer, 0, _receiveBufferOffset - bytesRead);

        _receiveBufferOffset -= bytesRead;
    }

    private void HandleError(SocketError socketError)
    {
        _events.OnSocketError(this, socketError);

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
                _flushLen = 0;
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
        _events.OnDisconnected(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            ArrayPool<byte>.Shared.Return(_receiveBuffer);
            ArrayPool<byte>.Shared.Return(_sendBuffer);
            ArrayPool<byte>.Shared.Return(_flushBuffer);
            ArrayPool<byte>.Shared.Return(_inflateBuffer);
            ArrayPool<byte>.Shared.Return(_deflateBuffer);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}