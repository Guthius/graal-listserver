using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet194 : IServerPacket
{
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(194);
    }
}