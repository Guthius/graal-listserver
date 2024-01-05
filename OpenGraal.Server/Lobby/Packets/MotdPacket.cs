using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record MotdPacket(string Message) : IServerPacket
{
    private const int Id = 2;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Message);
    }
}