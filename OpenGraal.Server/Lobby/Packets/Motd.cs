using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record Motd(string Message) : IPacket
{
    private const int Id = 2;
    
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(Id);
        writer.WriteStr(Message);
    }
}