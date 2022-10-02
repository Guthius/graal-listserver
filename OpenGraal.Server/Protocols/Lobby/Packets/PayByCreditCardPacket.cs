using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

internal sealed class PayByCreditCardPacket : IServerPacket
{
    private const int Id = 5;
    
    public string Url { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Url);
    }
}