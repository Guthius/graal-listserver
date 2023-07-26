using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record DisconnectPacket(string Message) : IServerPacket
{
    private const int Id = 16;
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Message);
    }
}