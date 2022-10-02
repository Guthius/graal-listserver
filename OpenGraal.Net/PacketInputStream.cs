namespace OpenGraal.Net;

public sealed class PacketInputStream : IPacketInputStream
{
    private readonly ReadOnlyMemory<byte> _memory;
    private int _pos;

    public PacketInputStream(ReadOnlyMemory<byte> memory)
    {
        _memory = memory;
        _pos = 1;
    }

    public byte ReadGChar()
    {
        var value = (byte)(_memory.Span[_pos] - 32);
        _pos++;
        return value;
    }

    public string ReadStr()
    {
        return ReadStr(_memory.Length - _pos);
    }
    
    public string ReadStr(int length)
    {
        var value = System.Text.Encoding.UTF8.GetString(_memory.Span.Slice(_pos, length));
        _pos += length;
        return value;
    }

    public string ReadNStr()
    {
        return ReadStr(ReadGChar());
    }
}