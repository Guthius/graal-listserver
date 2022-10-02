using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby;

public class PayByPhonePacket : IServerPacket
{
    private const int Id = 6;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr("1");
    }
}