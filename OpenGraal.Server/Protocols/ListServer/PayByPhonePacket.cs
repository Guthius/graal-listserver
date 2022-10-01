using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public class PayByPhonePacket : IPacket
{
    public byte Id => 6;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString("1");
    }
}