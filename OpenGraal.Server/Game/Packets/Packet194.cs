using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet194 : IPacket
{
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(194);
    }
}