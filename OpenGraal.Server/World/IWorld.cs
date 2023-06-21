using OpenGraal.Net;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.World;

public interface IWorld
{
    Player? CreatePlayer(Connection connection, string accountName);
}