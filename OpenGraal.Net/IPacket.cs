namespace OpenGraal.Net;

public interface IPacket
{
    byte Id { get; }
    void WriteTo(IPacketOutputStream output);
    
    // void ReadFrom(IPacketInputStream input);
}