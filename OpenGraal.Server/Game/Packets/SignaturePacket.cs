using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record SignaturePacket : IPacket
{
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(25);
        writer.WriteGChar(73);
    }
}