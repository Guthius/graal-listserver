using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record Packet190 : IServerPacket
{
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(190);
    }
}