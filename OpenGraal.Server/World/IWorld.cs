using OpenGraal.Net;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.World;

public interface IWorld
{
    Player? CreatePlayer(IConnection connection, string accountName);

    void SetLanguage(Player player, string language);
}