using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet168 : IServerPacket
{
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(168);
    }
}