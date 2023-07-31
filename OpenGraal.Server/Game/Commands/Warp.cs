using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record Warp(float X, float Y, string LevelName) : ICommand<Warp>
{
    public static Warp ReadFrom(Packet packet)
    {
        return new Warp(
            packet.ReadGChar() / 2.0f,
            packet.ReadGChar() / 2.0f,
            packet.ReadStr());
    }
}