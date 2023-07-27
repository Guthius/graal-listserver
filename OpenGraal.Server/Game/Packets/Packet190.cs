using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet190 : IPacket
{
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(190);
    }
}