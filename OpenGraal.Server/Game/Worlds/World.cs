using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenGraal.Data;
using OpenGraal.Net;
using OpenGraal.Server.Game.Players;

namespace OpenGraal.Server.Game.Worlds;

public sealed class World : BackgroundService
{
    private const int MinTickRate = 10;

    private readonly ILogger<World> _logger;
    private readonly WorldOptions _options = new();
    private readonly List<Player> _players = new();

    public World(ILogger<World> logger, IConfiguration configuration)
    {
        _logger = logger;

        configuration.GetSection("World").Bind(_options);

        if (_options.TickRate <= MinTickRate)
        {
            _options.TickRate = MinTickRate;
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
        lock (_players)
        {
            return _players.Any(player => player.AccountName
                .Equals(accountName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Player? CreatePlayer(GameUser user, string accountName)
    {
        if (IsAccountConnected(accountName))
        {
            return null;
        }

        var player = new Player(this, user)
        {
            AccountName = accountName
        };

        player.SendPropertiesToSelf(PlayerPropertySet.Init);
        
        lock (_players)
        {
            _players.Add(player);
            
            foreach (var other in _players)
            {
                if (other == player)
                {
                    continue;
                }

                other.SendPropertiesTo(player, PlayerPropertySet.InitOthers);
                
                player.SendPropertiesTo(other, PlayerPropertySet.InitOthers);
            }
        }
        
        _logger.LogInformation(
            "{AccountName} has entered the world", 
            accountName);
        
        return player;
    }

    public void DestroyPlayer(Player player)
    {
        lock (_players)
        {
            _players.Remove(player);
        }

        player.SendPropertiesToAll(PlayerProperty.Disconnected);
        
        _logger.LogInformation(
            "{AccountName} has left the world", 
            player.AccountName);
    }

    public Player? GetPlayer(int id)
    {
        lock (_players)
        {
            return _players.FirstOrDefault(x => x.Id == id);
        }
    }
    
    public void SendTo(Action<Packet> packet, Func<Player, bool> predicate)
    {
        lock (_players)
        {
            foreach (var player in _players.Where(predicate))
            {
                player.Send(packet);
            }
        }
    }

    public void SendToAll(Action<Packet> packet)
    {
        lock (_players)
        {
            foreach (var player in _players)
            {
                player.Send(packet);
            }
        }
    }

    private readonly Dictionary<string, WorldLevel> _levels = new();

    public WorldLevel GetLevel(string levelName)
    {
        levelName = levelName.ToLowerInvariant();

        lock (_levels)
        {
            if (_levels.TryGetValue(levelName, out var level))
            {
                return level;
            }
        }

        return LoadLevel(levelName);
    }

    private WorldLevel LoadLevel(string levelName)
    {
        var level = Level.LoadNw(levelName);

        if (level is null)
        {
            throw new Exception($"Could not load level {levelName}");
        }

        var serverLevel = new WorldLevel(this, level, levelName);

        lock (_levels)
        {
            _levels[levelName] = serverLevel;
        }

        return serverLevel;
    }

    public static int GetTime()
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() - 11078 * 24 * 60 * 60;

        return (int) (timestamp / 5);
    }
}