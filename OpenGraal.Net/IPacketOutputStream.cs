namespace OpenGraal.Net;

public interface IPacketOutputStream
{
    void WriteByte(int value);
    void WriteGChar(int value);
    void WriteStr(string str);
    void WriteNStr(string str);
}