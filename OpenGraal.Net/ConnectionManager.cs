using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenGraal.Net;

public sealed class ConnectionManager<TProtocol> where TProtocol : IProtocol
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<int> _sessionIds = new();
    private readonly Dictionary<int, IServiceScope> _sessionScopes = new();
    
    public ConnectionManager(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var maxSessions = configuration.GetValue<int>("MaxSessions");
        if (maxSessions < 1)
        {
            maxSessions = 100;
        }

        // We reserve session ID's starting at ID 2.
        // ID's 0 and 1 are reserved for the server itself (0) and for the NPC-server (1).
        
        for (var i = maxSessions; i > 0; --i)
        {
            _sessionIds.Push(1 + i);
        }
    }

    public void Create(IServiceEvents handler, Socket socket)
    {
        if (_sessionIds.Count == 0)
        {
            return;
        }

        var id = _sessionIds.Pop();
        var scope = _serviceProvider.CreateScope();
        var protocol = scope.ServiceProvider.GetRequiredService<TProtocol>();
        var session = new Connection(id, handler, protocol, socket);

        _sessionScopes[id] = scope;
    }
    
    public void Destroy(int sessionId)
    {
        if (!_sessionScopes.TryGetValue(sessionId, out var scope))
        {
            return;
        }
        
        _sessionScopes.Remove(sessionId);
        _sessionIds.Push(sessionId);
        
        scope.Dispose();
    }
}