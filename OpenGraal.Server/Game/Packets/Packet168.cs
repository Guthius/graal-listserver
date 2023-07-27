using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet168 : IPacket
{
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(168);
    }
}