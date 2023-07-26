using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet103 : IServerPacket
{
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(103);
        output.WriteStr(" *");
    }
}