using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record ShowMore(string Url) : IPacket
{
    private const int Id = 3;
    
    public void WriteTo(Packet packet)
    {
        packet.WriteGChar(Id);
        packet.WriteStr(Url);
    }
}