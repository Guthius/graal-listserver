using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public class ShowMorePacket : IPacket
{
    public byte Id => 3;
    public string Url { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString(Url);
    }
}