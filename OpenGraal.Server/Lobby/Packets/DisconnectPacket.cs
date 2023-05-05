using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record DisconnectPacket(string Message) : IServerPacket
{
    private const int Id = 4;
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Message);
    }
}