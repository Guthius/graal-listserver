using OpenGraal.Net;
using OpenGraal.Server.Game.Commands;

namespace OpenGraal.Server.Game;

internal sealed class GameCommandParser : CommandParser<GameUser>
{
    public GameCommandParser()
    {
        Bind<SetPlayerProperties>(2, SetPlayerProperties);
        Bind<GetFile>(23, GetFile);
        Bind<ShowImage>(24, ShowImage);
        Bind<SetLanguage>(37, SetLanguage);
        Bind<GetMapInfo>(39, GetMapInfo);
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
        if (user.ClientType == GameUserType.Await)
        {
            HandleLogin(user, packet);

            return;
        }

        if (user.Player is null)
        {
            user.Disconnect("An unknown problem occured during your session.");

            return;
        }

        RemoveInjectedByteFromPacket(user, packet);

        base.Handle(user, packet);
    }

    private static void HandleLogin(GameUser user, Packet packet)
    {
        var clientType = (GameUserType) (1 << packet.ReadGChar());

        var command = Login.ReadFrom(packet);

        user.Login(
            command.Key,
            command.AccountName,
            command.Password,
            clientType,
            command.ClientVersion);
    }

    private static void SetPlayerProperties(GameUser user, SetPlayerProperties command)
    {
        user.SetPlayerProperties(command.Properties);
    }
    
    private static void GetFile(GameUser user, GetFile command)
    {
        user.SendFile(command.FileName);
    }
    
    private static void ShowImage(GameUser user, ShowImage command)
    {
        user.ShowImage(command.Data);
    }
    
    private static void SetLanguage(GameUser user, SetLanguage command)
    {
        user.SetLanguage(command.Language);
    }
    
    private static void GetMapInfo(GameUser user, GetMapInfo command)
    {
        // Don't know what this does exactly.
        // Might be GMap related.
    }
}