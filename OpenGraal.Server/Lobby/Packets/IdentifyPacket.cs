using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record IdentifyPacket(
        string ClientVersion) 
    : IClientPacket<IdentifyPacket>
{
    public static IdentifyPacket ReadFrom(IPacketInputStream input)
    {
        return new IdentifyPacket(
            input.ReadStr());
    }
}