namespace OpenGraal.Net;

public interface IPacketOutputStream
{
    void WriteByte(byte value);
    void WriteBytes(ReadOnlySpan<byte> bytes);
    void WriteGChar(byte value);
    void WriteString(string str);
    void WriteNStr(string str);
}