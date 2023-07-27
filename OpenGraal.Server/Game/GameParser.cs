using OpenGraal.Net;
using OpenGraal.Server.Game.Commands;
using OpenGraal.Server.Game.Packets;

namespace OpenGraal.Server.Game;

internal sealed class GameParser : CommandParser<GameUser>
{
    public GameParser()
    {
        Bind<SetLanguage>(37, SetLanguage);
    }

    private static void RemoveInjectedByteFromPacket(GameUser user, Packet packet)
    {
        user.Iterator *= 0x8088405;
        user.Iterator += user.Key;

        var pos = (int) (user.Iterator & 0x0FFFF) % packet.Length;

        packet.Remove(pos, 1);
    }

    public override void Handle(GameUser user, Packet packet)
    {
        /*
         * First packet that arrives will always be the login packet. We need to handle this
         * here because the login packet does not contain a packet ID.
         */
        if (user.ClientType == ClientType.Await)
        {
            HandleLogin(user, packet);

            return;
        }

        if (user.Player is null)
        {
            user.Send(new DisconnectPacket(
                "An unknown problem occured during your session."));

            user.Disconnect();

            return;
        }

        RemoveInjectedByteFromPacket(user, packet);

        base.Handle(user, packet);
    }

    private static void HandleLogin(GameUser user, Packet packet)
    {
        var clientType = (ClientType) (1 << packet.ReadGChar());

        var command = Login.ReadFrom(packet);

        user.Login(
            command.Key,
            command.AccountName,
            command.Password,
            clientType,
            command.ClientVersion);
    }

    private static void SetLanguage(GameUser user, SetLanguage packet)
    {
        user.SetLanguage(packet.Language);
    }
}