using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet103 : IPacket
{
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(103);
        writer.WriteStr(" *");
    }
}