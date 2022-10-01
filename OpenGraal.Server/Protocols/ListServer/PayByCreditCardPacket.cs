using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public class PayByCreditCardPacket : IPacket
{
    public byte Id => 5;
    public string Url { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString(Url);
    }
}