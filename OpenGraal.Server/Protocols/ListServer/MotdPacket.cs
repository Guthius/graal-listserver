using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.ListServer;

public class MotdPacket : IPacket
{
    public byte Id => 2;
    public string Message { get; set; } = string.Empty;

    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteString(Message);
    }
}