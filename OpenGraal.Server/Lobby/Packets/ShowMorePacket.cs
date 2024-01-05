using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record ShowMorePacket(string Url) : IServerPacket
{
    private const int Id = 3;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Url);
    }
}