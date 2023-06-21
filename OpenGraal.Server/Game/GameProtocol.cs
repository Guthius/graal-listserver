using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.World;

namespace OpenGraal.Server.Game;

internal sealed class GameProtocol : Protocol
{
    private readonly ILogger<GameProtocol> _logger;
    private readonly IWorld _world;

    public GameProtocol(
        ILogger<GameProtocol> logger, 
        IWorld world) 
        : base(logger)
    {
        _logger = logger;
        _world = world;
    }
}