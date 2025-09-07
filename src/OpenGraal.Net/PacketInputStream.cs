namespace OpenGraal.Net;

public sealed class PacketInputStream(ReadOnlyMemory<byte> memory) : IPacketInputStream
{
    private int _pos = 1;

    public byte ReadGChar()
    {
        var value = (byte) (memory.Span[_pos] - 32);
        _pos++;
        return value;
    }

    public string ReadStr()
    {
        return ReadStr(memory.Length - _pos);
    }

    public string ReadStr(int length)
    {
        var value = System.Text.Encoding.UTF8.GetString(memory.Span.Slice(_pos, length));
        _pos += length;
        return value;
    }

    public string ReadNStr()
    {
        return ReadStr(ReadGChar());
    }
}