namespace OpenGraal.Net;

public sealed class PacketOutputStream(byte[] buffer) : IPacketOutputStream
{
    private int _pos;

    public void Flush(IPacketEncoding encoding, byte[] output, out int outputLen)
    {
        if (_pos == 0)
        {
            outputLen = 0;

            return;
        }

        encoding.Encode(buffer, 0, _pos, output, out outputLen);

        _pos = 0;
    }

    public void WriteByte(int value)
    {
        buffer[_pos] = (byte) value;
        _pos++;
    }

    public void WriteGChar(int value)
    {
        buffer[_pos] = (byte) (value + 32);
        _pos++;
    }

    public void WriteStr(string str)
    {
        var len = str.Length;
        System.Text.Encoding.UTF8.GetBytes(str, 0, len, buffer, _pos);
        _pos += len;
    }

    public void WriteNStr(string str)
    {
        // TODO: Truncate string if needed...

        WriteGChar((byte) str.Length);
        WriteStr(str);
    }
}