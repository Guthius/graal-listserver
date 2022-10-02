using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

internal sealed class IdentifyPacket : IClientPacket
{
    public string ClientVersion { get; set; } = string.Empty;

    public void ReadFrom(IPacketInputStream input)
    {
        ClientVersion = input.ReadStr();
    }
}