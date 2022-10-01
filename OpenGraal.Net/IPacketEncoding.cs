namespace OpenGraal.Net;

public interface IPacketEncoding
{
    void Encode(byte[] bytes, int offset, int count, byte[] dest, out int destLen);
}