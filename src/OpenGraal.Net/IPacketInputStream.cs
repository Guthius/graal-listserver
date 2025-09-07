namespace OpenGraal.Net;

public interface IPacketInputStream
{
    byte ReadGChar();
    string ReadStr();
    string ReadStr(int length);
    string ReadNStr();
}