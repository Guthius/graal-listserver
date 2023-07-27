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

    public Packet()
    {
    }

    public Packet(byte[] bytes, int offset, int size)
    {
        SetBuffer(bytes, offset, size);
    }

    public void SetBuffer(byte[] bytes, int offset, int size)
    {
        _bytes = bytes;
        _start = _read = _write = offset;
        _end = offset + size;
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

    public void WriteByte(byte value)
    {
        ThrowIfNotEnoughSpace(sizeof(byte));

        _bytes[_write] = value;
        _write++;
    }

    public void WriteGChar(int value)
    {
        ThrowIfNotEnoughSpace(sizeof(byte));

        _bytes[_write] = (byte) (value + 32);
        _write++;
    }

    public void WriteStr(string str)
    {
        ThrowIfNotEnoughSpace(str.Length);

        var len = str.Length;
        System.Text.Encoding.UTF8.GetBytes(str, 0, len, _bytes, _write);
        _write += len;
    }

    public void WriteNStr(string str)
    {
        ThrowIfNotEnoughSpace(1 + str.Length);

        // TODO: Truncate string if needed...

        WriteGChar((byte) str.Length);
        WriteStr(str);
    }
    
    public byte ReadGChar()
    {
        var value = (byte) (_bytes[_read] - 32);
        _read++;
        return value;
    }

    public string ReadStr()
    {
        return ReadStr(_end - _read);
    }

    public string ReadStr(int length)
    {
        var value = System.Text.Encoding.UTF8.GetString(_bytes, _read, length);
        _read += length;
        return value;
    }

    public string ReadNStr()
    {
        return ReadStr(ReadGChar());
    }
}