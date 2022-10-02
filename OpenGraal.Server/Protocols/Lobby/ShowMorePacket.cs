using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby;

public class ShowMorePacket : IServerPacket
{
    private const int Id = 3;
    
    public string Url { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteStr(Url);
    }
}