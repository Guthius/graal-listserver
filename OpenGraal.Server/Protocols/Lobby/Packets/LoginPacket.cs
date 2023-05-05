using OpenGraal.Net;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

internal sealed record LoginPacket(
        string AccountName,
        string Password) 
    : IClientPacket<LoginPacket>
{
    public static LoginPacket ReadFrom(IPacketInputStream input)
    {
        return new LoginPacket(
            input.ReadNStr(), 
            input.ReadNStr());
    }
}