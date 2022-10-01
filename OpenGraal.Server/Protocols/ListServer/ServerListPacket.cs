using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public class ServerListPacket : IPacket
{
    public byte Id => 0;
    public string Data { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString(Data);
    }
}