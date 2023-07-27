using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Commands;

internal sealed record Login(string AccountName, string Password) : ICommand<Login>
{
    public static Login ReadFrom(Packet packet)
    {
        return new Login(
            packet.ReadNStr(),
            packet.ReadNStr());
    }
}