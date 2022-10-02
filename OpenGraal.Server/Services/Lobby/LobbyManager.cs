using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpenGraal.Server.Services.Lobby;

internal sealed class LobbyManager
{
    private readonly ILogger<LobbyManager> _logger;
    private readonly List<ServerInfo> _serverInfos = new();
    
    public LobbyManager(ILogger<LobbyManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        var path = configuration["DataPath"];
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception(
                "Unable to load serverlist. " +
                "Please configure 'DataPath' and restart.");
        }
        
        path = Path.Combine(path, "servers.json");
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            
            var items = JsonSerializer.Deserialize<List<ServerInfo>>(json);
            if (items is not null)
            {
                _serverInfos.AddRange(items);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load serverlist");
        }
    }
    
    public List<ServerInfo> GetServerInfos()
    {
        return _serverInfos;
    }
}