using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

internal sealed record LoginPacket(
        byte Key,
        string ClientVersion,
        string AccountName,
        string Password) 
    : IClientPacket<LoginPacket>
{
    public static LoginPacket ReadFrom(IPacketInputStream input)
    {
        return new LoginPacket(
            input.ReadGChar(),
            input.ReadStr(8),
            input.ReadNStr(),
            input.ReadNStr());
    }
}