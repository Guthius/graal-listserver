using OpenGraal.Server.Game;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.World;

internal interface IWorld
{
    Player? CreatePlayer(GameUser user, string accountName);
    void SetLanguage(Player player, string language);
}