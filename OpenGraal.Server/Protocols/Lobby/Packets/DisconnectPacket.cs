using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

public sealed class DisconnectPacket : IServerPacket
{
    private const int Id = 4;
    
    public string Message { get; set; } = string.Empty;
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Message);
    }
}