using System.Text;
using OpenGraal.Net;

namespace OpenGraal.Server;

public class PacketOutputStream : IPacketOutputStream
{
    private readonly byte[] _buffer;
    private int _pos;

    public PacketOutputStream(byte[] buffer)
    {
        _buffer = buffer;
        _pos = 0;
    }

    public void Flush(IPacketEncoding encoding, byte[] output, out int outputLen)
    {
        if (_pos == 0)
        {
            outputLen = 0;

            return;
        }

        encoding.Encode(_buffer, 0, _pos, output, out outputLen);

        _pos = 0;
    }

    public void WriteByte(byte value)
    {
        _buffer[_pos] = value;
        _pos++;
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        bytes.CopyTo(_buffer.AsSpan(_pos));
    }

    public void WriteGChar(byte value)
    {
        _buffer[_pos] = (byte)(value + 32);
        _pos++;
    }

    public void WriteString(string str)
    {
        var len = str.Length;
        Encoding.UTF8.GetBytes(str, 0, len, _buffer, _pos);
        _pos += len;
    }

    public void WriteNStr(string str)
    {
        // TODO: Truncate string if needed...

        WriteGChar((byte)str.Length);
        WriteString(str);
    }
}