namespace OpenGraal.Net;

public interface IPacketEncoding
{
    int Decode(byte[] bytes, int offset, int count, byte[] dest, out int destLen);
    void Encode(byte[] bytes, int offset, int count, byte[] dest, out int destLen);
}