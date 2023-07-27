using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Commands;

internal sealed record Identify(string ClientVersion) : ICommand<Identify>
{
    public static Identify ReadFrom(Packet reader)
    {
        return new Identify(
            reader.ReadStr());
    }
}