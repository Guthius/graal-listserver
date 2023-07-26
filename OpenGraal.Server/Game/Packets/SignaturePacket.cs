using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record SignaturePacket : IServerPacket
{
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(25);
        output.WriteGChar(73);
    }
}