using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

internal sealed class LoginPacket : IClientPacket
{
    public string AccountName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public void ReadFrom(IPacketInputStream input)
    {
        AccountName = input.ReadNStr();
        Password = input.ReadNStr();
    }
}