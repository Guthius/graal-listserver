using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

public class MotdPacket : IServerPacket
{
    private const int Id = 2;
    
    public string Message { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Message);
    }
}