using System.Windows.Input;
using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record WarpIfModified(DateTimeOffset LastModified, float X, float Y, string LevelName) : ICommand<WarpIfModified>
{
    public static WarpIfModified ReadFrom(Packet packet)
    {
        var lastModified = packet.ReadGInt5();
        var lastModifiedDate = DateTimeOffset.FromUnixTimeSeconds(lastModified);

        return new WarpIfModified(
            lastModifiedDate,
            packet.ReadGChar() / 2.0f,
            packet.ReadGChar() / 2.0f,
            packet.ReadStr());
    }
}