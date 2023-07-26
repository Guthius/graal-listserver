using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

internal sealed record LanguagePacket(string Language)
    : IClientPacket<LanguagePacket>
{
    public static LanguagePacket ReadFrom(IPacketInputStream input)
    {
        return new LanguagePacket(input.ReadStr());
    }
}