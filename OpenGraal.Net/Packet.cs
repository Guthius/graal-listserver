using System.Runtime.InteropServices;
using System.Text;

namespace OpenGraal.Net;

public sealed record Packet
{
    private byte[] _bytes = Array.Empty<byte>();
    private int _start;
    private int _read;
    private int _write;
    private int _end;

    public int BytesRead => _read - _start;
    public int BytesWritten => _write - _start;
    public int Length => _end - _start;

    public void SetBuffer(byte[] bytes, int offset, int size)
    {
        _bytes = bytes;
        _start = _read = _write = offset;
        _end = offset + size;
    }

    public void Dump()
    {
        var bytes = _bytes.AsSpan(_start, _end - _start).ToArray();
        
        File.WriteAllBytes("PacketDump.bin", bytes);
    }

    public void Remove(int offset, int count)
    {
        var buf = _bytes.AsSpan(_start, _end - _start);

        var src = buf[(offset + count)..];
        var dest = buf[offset..];

        if (src.Length > 0)
        {
            src.CopyTo(dest);
        }

        _end -= count;
    }

    private void ThrowIfNotEnoughSpace(int space)
    {
        if (_end - _write < space)
        {
            throw new Exception("out of space");
        }
    }

    public Packet WriteByte(byte value)
    {
        ThrowIfNotEnoughSpace(sizeof(byte));

        _bytes[_write] = value;
        _write++;

        return this;
    }

    public Packet WriteBytes(ReadOnlySpan<byte> bytes)
    {
        ThrowIfNotEnoughSpace(bytes.Length);

        bytes.CopyTo(_bytes.AsSpan(_write, _end - _write));
        
        _write += bytes.Length;

        return this;
    }
    
    public Packet WriteBytes(byte[] bytes, int index, int count)
    {
        ThrowIfNotEnoughSpace(count);

        var span = bytes.AsSpan(index, count);
        
        span.CopyTo(_bytes.AsSpan(_write, _end - _write));
        
        _write += bytes.Length;

        return this;
    }
    
    public Packet WriteRaw<T>(ReadOnlySpan<T> data) where T : struct
    {
        var bytes = MemoryMarshal.AsBytes(data);

        return WriteBytes(bytes);
    }
    
    public Packet WriteRaw<T>(T[] data) where T : struct
    {
        var bytes = MemoryMarshal.AsBytes(data.AsSpan());

        return WriteBytes(bytes);
    }

    public Packet WriteGChar(int value)
    {
        ThrowIfNotEnoughSpace(sizeof(byte));

        _bytes[_write] = (byte) (value + 32);
        _write++;

        return this;
    }

    public Packet WriteGShort(int value)
    {
        ThrowIfNotEnoughSpace(2);

        value &= 0x3FFF;
        
        _bytes[_write] = (byte) (((value >> 7) & 0x7f) + 32);
        _bytes[_write + 1] = (byte) ((value & 0x7f) + 32);
        
        _write += 2;

        return this;
    }
    
    public Packet WriteGInt(int value)
    {
        ThrowIfNotEnoughSpace(3);

        value &= 0x1FFFFF;
        
        _bytes[_write] = (byte) (((value >> 14) & 0x7f) + 32);
        _bytes[_write + 1] = (byte) (((value >> 7) & 0x7f) + 32);
        _bytes[_write + 2] = (byte) ((value & 0x7f) + 32);
        
        _write += 3;

        return this;
    }
    
    public Packet WriteGInt4(int value)
    {
        ThrowIfNotEnoughSpace(4);

        value &= 0xFFFFFFF;
        
        _bytes[_write] = (byte) (((value >> 21) & 0x7f) + 32);
        _bytes[_write + 1] = (byte) (((value >> 14) & 0x7f) + 32);
        _bytes[_write + 2] = (byte) (((value >> 7) & 0x7f) + 32);
        _bytes[_write + 3] = (byte) ((value & 0x7f) + 32);
        
        _write += 4;

        return this;
    }
    
    public Packet WriteGInt5(long value)
    {
        ThrowIfNotEnoughSpace(5);

        value &= 0x7FFFFFFFF;
        
        _bytes[_write] = (byte) (((value >> 28) & 0x7f) + 32);
        _bytes[_write + 1] = (byte) (((value >> 21) & 0x7f) + 32);
        _bytes[_write + 2] = (byte) (((value >> 14) & 0x7f) + 32);
        _bytes[_write + 3] = (byte) (((value >> 7) & 0x7f) + 32);
        _bytes[_write + 4] = (byte) ((value & 0x7f) + 32);
        
        _write += 5;

        return this;
    }
    
    public Packet WriteStr(ReadOnlySpan<char> str)
    {
        ThrowIfNotEnoughSpace(str.Length);

        var len = Encoding.UTF8.GetBytes(str, _bytes.AsSpan(_write));
        
        _write += len;

        return this;
    }
    
    public Packet WriteNStr(ReadOnlySpan<char> str, byte maxLength = 223)
    {
        if (str.Length > maxLength)
        {
            str = str[..maxLength];
        }
        
        ThrowIfNotEnoughSpace(1 + str.Length);
        
        WriteGChar((byte) str.Length);
        WriteStr(str);

        return this;
    }

    public Packet Write(Action<Packet> writer)
    {
        writer(this);

        return this;
    }
    
    private void ThrowIfNotEnoughBytesLeft(int count)
    {
        if (_end - _read < count)
        {
            throw new Exception("end of packet");
        }
    }
    
    public byte ReadByte()
    {
        ThrowIfNotEnoughBytesLeft(sizeof(byte));
        
        return _bytes[_read++];
    }

    public byte[] ReadBytes(int count)
    {
        count = Math.Min(count, _end - _read);
        if (count == 0)
        {
            return Array.Empty<byte>();
        }

        var value = _bytes.AsSpan(_read, count).ToArray();

        _read += count;

        return value;
    }

    public byte[] ReadBytes()
    {
        return ReadBytes(_end - _read);
    }
    
    public byte ReadGChar()
    {
        ThrowIfNotEnoughBytesLeft(sizeof(byte));
        
        return (byte) (_bytes[_read++] - 32);
    }
    
    public int ReadGShort()
    {
        ThrowIfNotEnoughBytesLeft(2);
        
        var value =
            ((_bytes[_read] - 32) << 7) |
            (_bytes[_read + 1] - 32);

        _read += 2;
        
        return value;
    }
    
    public int ReadGInt()
    {
        ThrowIfNotEnoughBytesLeft(3);
        
        var value =
            ((_bytes[_read] - 32) << 14) |
            ((_bytes[_read + 1] - 32) << 7) |
            (_bytes[_read + 2] - 32);

        _read += 3;
        
        return value;
    }
    
    public int ReadGInt4()
    {
        ThrowIfNotEnoughBytesLeft(4);
        
        var value =
            ((_bytes[_read] - 32) << 21) |
            ((_bytes[_read + 1] - 32) << 14) |
            ((_bytes[_read + 2] - 32) << 7) |
            (_bytes[_read + 3] - 32);

        _read += 4;
        
        return value;
    }
    
    public int ReadGInt5()
    {
        ThrowIfNotEnoughBytesLeft(5);
        
        var value =
            ((_bytes[_read] - 32) << 28) |
            ((_bytes[_read + 1] - 32) << 21) |
            ((_bytes[_read + 2] - 32) << 14) |
            ((_bytes[_read + 3] - 32) << 7) |
            (_bytes[_read + 4] - 32);

        _read += 5;
        
        return value;
    }
    
    public string ReadStr()
    {
        return ReadStr(_end - _read);
    }

    public string ReadStr(int size)
    {
        size = Math.Min(size, _end - _read);
        
        var value = Encoding.UTF8.GetString(_bytes, _read, size);
        
        _read += size;
        
        return value;
    }

    public string ReadNStr()
    {
        return ReadStr(ReadGChar());
    }
}