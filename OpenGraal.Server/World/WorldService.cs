using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.World;

internal sealed class WorldService : BackgroundService, IWorld
{
    private readonly ILogger<WorldService> _logger;
    private readonly WorldOptions _options = new();

    public WorldService(ILogger<WorldService> logger, IConfiguration configuration)
    {
        _logger = logger;

        configuration.GetSection("World").Bind(_options);
        
        if (_options.TickRate <= 10)
        {
            _options.TickRate = 10;
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var millisecondsPerTick = 1000 / _options.TickRate;
        
        _logger.LogInformation(
            "World simulation started with a tick rate of {TickRate}",
            _options.TickRate);
        
        var stopWatch = new Stopwatch();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            stopWatch.Start();
            
            Tick();
            
            stopWatch.Stop();

            var timeToWait = millisecondsPerTick - (int) stopWatch.ElapsedMilliseconds;
            if (timeToWait > 0)
            {
                await Task.Delay(timeToWait, stoppingToken);
            }
        }
    }

    private static void Tick()
    {
    }

    private bool IsAccountConnected(string accountName)
    {
        return false;
    }
    
    public Player? CreatePlayer(Connection connection, string accountName)
    {
        if (IsAccountConnected(accountName))
        {
            return null;
        }
        
        throw new NotImplementedException();
    }
}