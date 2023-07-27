using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record Disconnect(string Message) : IPacket
{
    private const int Id = 4;
    
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(Id);
        writer.WriteStr(Message);
    }
}