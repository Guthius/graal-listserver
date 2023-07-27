using OpenGraal.Net;
using OpenGraal.Server.Lobby.Commands;
using OpenGraal.Server.Lobby.Packets;

namespace OpenGraal.Server.Lobby;

internal sealed class LobbyParser : CommandParser<LobbyUser>
{
    public LobbyParser()
    {
        Bind<Identify>(0, OnIdentify);
        Bind<Login>(1, OnLogin);
    }
    
    private static void OnIdentify(LobbyUser user, Identify packet)
    {
        if (packet.ClientVersion != "newmain")
        {
            user.Send(new Disconnect("You are using a unsupported client."));
        }
    }

    private static void OnLogin(LobbyUser user, Login packet)
    {
        user.Login(packet.AccountName, packet.Password);
    }
}