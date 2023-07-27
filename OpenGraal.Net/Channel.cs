using System.Buffers;
using System.Net;
using System.Net.Sockets;
using OpenGraal.Net.Compression;

namespace OpenGraal.Net;

public sealed class Channel
{
    private const int BufferSize = 204800;
    private const byte PacketDelimiter = 0x0A;

    private readonly IChannelEventListener _listener;
    private bool _connected;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ZLibCompression _compression = new();
    private readonly Packet _packetOut = new();
    private readonly Packet _packetIn = new();
    private readonly byte[] _buffer;
    private readonly int _bufferInOffset;
    private int _bufferInLen;
    private readonly int _bufferSendOffset;
    private readonly int _bufferOutOffset;
    private int _bufferOutLen;
    private readonly int _bufferInflateOffset;
    private readonly Socket _socket;
    private readonly SocketAsyncEventArgs _socketReceiveArgs;
    private readonly SocketAsyncEventArgs _socketSendArgs;
    private bool _sending;
    private bool _disposed;

    public int Id { get; }
    public string Address { get; }

    public Channel(int id, IChannelEventListener listener, User user, Socket socket)
    {
        Id = id;
        Address = socket.RemoteEndPoint switch
        {
            IPEndPoint ip => ip.Address.ToString(),
            _ => string.Empty
        };

        _listener = listener;
        _connected = true;

        _buffer = ArrayPool<byte>.Shared.Rent(BufferSize * 5);
        _bufferInOffset = BufferSize;
        _bufferInLen = 0;
        _bufferSendOffset = BufferSize * 2;
        _bufferOutOffset = BufferSize * 3;
        _bufferOutLen = 0;
        _bufferInflateOffset = BufferSize * 4;

        _socket = socket;
        _socketReceiveArgs = new SocketAsyncEventArgs();
        _socketReceiveArgs.Completed += HandleIo;
        _socketReceiveArgs.SetBuffer(_buffer, 0, BufferSize);
        _socketSendArgs = new SocketAsyncEventArgs();
        _socketSendArgs.Completed += HandleIo;
    }

    internal void Begin()
    {
        _listener.Connected(this);
        
        BeginReceive();
    }
    
    public void Send(IPacket packet)
    {
        if (!_connected)
        {
            return;
        }

        _lock.EnterWriteLock();

        var bufferOffset = _bufferOutOffset + _bufferOutLen;
        var bufferSize = BufferSize - _bufferOutLen;

        _packetOut.SetBuffer(_buffer, bufferOffset, bufferSize);

        packet.WriteTo(_packetOut);

        _packetOut.WriteByte(PacketDelimiter);

        _bufferOutLen += _packetOut.BytesWritten;
        
        _lock.ExitWriteLock();

        if (_sending)
        {
            return;
        }

        _sending = true;

        BeginSend();
    }

    private void BeginReceive()
    {
        if (!_connected)
        {
            return;
        }

        var pending = _socket.ReceiveAsync(_socketReceiveArgs);

        if (!pending)
        {
            HandleReceive(_socketReceiveArgs);
        }
    }

    private void BeginSend()
    {
        var pending = false;

        while (!pending)
        {
            pending = SendFromBuffer();
        }
    }

    private bool SendFromBuffer()
    {
        var size = _bufferOutLen;

        if (size == 0)
        {
            _sending = false;

            return true;
        }

        _lock.EnterWriteLock();

        var clen = _compression.Compress(
            _buffer,
            _bufferOutOffset,
            _bufferOutLen,
            _buffer,
            _bufferSendOffset + 2,
            BufferSize - 2);

        _buffer[_bufferSendOffset] = (byte) ((clen >> 8) & 0xFF);
        _buffer[_bufferSendOffset + 1] = (byte) (clen & 0xFF);

        size = 2 + clen;

        _bufferOutLen = 0;

        _lock.ExitWriteLock();

        _socketSendArgs.SetBuffer(_buffer, _bufferSendOffset, size);

        return _socket.SendAsync(_socketSendArgs);
    }

    private void HandleIo(object? sender, SocketAsyncEventArgs e)
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                HandleReceive(e);
                break;

            case SocketAsyncOperation.Send:
                HandleSend(e);
                break;
        }
    }

    private void HandleReceive(SocketAsyncEventArgs e)
    {
        var pending = false;

        while (!pending)
        {
            if (e.SocketError != SocketError.Success)
            {
                HandleError(e.SocketError);

                return;
            }

            if (e.BytesTransferred <= 0)
            {
                Close();

                return;
            }

            var bytes = e.Buffer.AsSpan(e.Offset, e.BytesTransferred);

            HandleReceivedBytes(bytes);

            try
            {
                pending = _socket.ReceiveAsync(e);
            }
            catch
            {
                return;
            }
        }
    }

    private void HandleReceivedBytes(ReadOnlySpan<byte> bytes)
    {
        var dest = _buffer.AsSpan(_bufferInOffset, BufferSize)[_bufferInLen..];
        
        if (dest.Length < bytes.Length)
        {
            Close();

            return;
        }

        bytes.CopyTo(dest);

        _bufferInLen += bytes.Length;

        if (_bufferInLen < 2)
        {
            return;
        }

        var offset = _bufferInOffset;
        var start = offset;
        var end = start + _bufferInLen;

        while (true)
        {
            var bytesLeft = end - offset;
            if (bytesLeft < 2)
            {
                break;
            }

            var len = (_buffer[offset] << 8) | _buffer[offset + 1];
            if (len + 2 > bytesLeft)
            {
                break;
            }

            var clen = _compression.Decompress(_buffer, offset + 2, len, _buffer, _bufferInflateOffset, BufferSize);
            if (clen > 0)
            {
                HandleReceivedPackets(_buffer, _bufferInflateOffset, clen);
            }

            offset += 2 + len;
        }

        var buf1 = _buffer.AsSpan(_bufferInOffset, BufferSize);
        var buf2 = _buffer.AsSpan(offset, _bufferInOffset + _bufferInLen - offset);

        if (buf2.Length > 0)
        {
            buf2.CopyTo(buf1);
        }

        _bufferInLen = buf2.Length;
    }

    private void HandleReceivedPackets(byte[] bytes, int offset, int count)
    {
        var end = offset + count;

        for (var i = offset; i < end; ++i)
        {
            if (bytes[i] != PacketDelimiter)
            {
                continue;
            }

            HandlePacket(bytes, offset, i - offset);

            offset = i + 1;
        }

        var len = end - offset;

        if (len > 0)
        {
            HandlePacket(bytes, offset, len);
        }
    }

    private void HandlePacket(byte[] bytes, int offset, int size)
    {
        _packetIn.SetBuffer(bytes, offset, size);

        _listener.Packet(this, _packetIn);
    }

    private void HandleSend(SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            HandleError(e.SocketError);

            return;
        }

        if (e.BytesTransferred > 0)
        {
            BeginSend();
        }
        else
        {
            Close();
        }
    }

    private void HandleError(SocketError socketError)
    {
        _listener.Error(this, socketError);

        Close();
    }

    public void Close()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!disposing)
        {
            return;
        }

        ArrayPool<byte>.Shared.Return(_buffer);

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
        _listener.Disconnected(this);
    }

    public void Dispose()
    {
        Dispose(true);
    }
}