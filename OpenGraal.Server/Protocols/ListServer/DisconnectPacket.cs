using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public sealed class DisconnectPacket : IPacket
{
    public byte Id => 4;
    public string Message { get; set; } = string.Empty;
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString(Message);
    }
}