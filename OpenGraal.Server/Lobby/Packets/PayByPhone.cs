using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record PayByPhone : IPacket
{
    private const int Id = 6;

    public void WriteTo(Packet packet)
    {
        packet.WriteGChar(Id);
        packet.WriteStr("1");
    }
}