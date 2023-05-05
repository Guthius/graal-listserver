using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record PayByCreditCardPacket(string Url) : IServerPacket
{
    private const int Id = 5;
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Url);
    }
}