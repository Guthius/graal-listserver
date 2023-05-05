using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpenGraal.Server.Lobby;

internal sealed class LobbyManager
{
    private readonly ILogger<LobbyManager> _logger;
    private readonly string _dataPath;
    private readonly List<ServerInfo> _serverInfos = new();
    
    public LobbyManager(ILogger<LobbyManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _dataPath = configuration["DataPath"];
        
        LoadServerList();
    }

    private void LoadServerList()
    {
        if (string.IsNullOrEmpty(_dataPath))
        {
            throw new Exception(
                "Unable to load serverlist. " +
                "Please configure 'DataPath' and restart.");
        }
        
        var path = Path.Combine(_dataPath, "servers.json");
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
            _logger.LogError(ex, "Failed to load serverlist");
        }
    }
    
    public List<ServerInfo> GetServerList()
    {
        return _serverInfos;
    }
}