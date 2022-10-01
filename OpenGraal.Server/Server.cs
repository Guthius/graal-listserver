using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenGraal.Net;
using OpenGraal.Server.Database;
using Serilog;

namespace OpenGraal.Server;

public class Server : BackgroundService, ISessionHandler
{
    private readonly IDatabase _database;
    private readonly ServerSettings _options;
    private readonly List<ISession> _sessions = new();
    private Socket? _socket;

    public Server(IDatabase database, IOptions<ServerSettings> options)
    {
        _database = database;
        _options = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, _options.Port));
        _socket.Listen((int)SocketOptionName.MaxConnections);

        Log.Information("The server is running on port {Port}", _options.Port);
        Log.Information("Listening for connections...");

        return base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Debug.Assert(_socket != null, nameof(_socket) + " != null");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await _socket.AcceptAsync(stoppingToken);

            var session = new Session(this, client, _database, _options);

            lock (_sessions)
            {
                _sessions.Add(session);
            }

            await Task.Delay(10, stoppingToken);
        }

        Log.Information("Server has stopped");
    }

    public void OnConnected(ISession session)
    {
        Log.Information("'{ClientIp}' has connected", session.Ip);
    }

    public void OnDisconnected(ISession session)
    {
        Log.Information("'{ClientIp}' has disconnected", session.Ip);

        lock (_sessions)
        {
            _sessions.Remove(session);
        }
    }

    public void OnSocketError(ISession session, SocketError socketError)
    {
        Log.Information("'{ClientIp}' socket error {ErrorCode}", session.Ip, socketError);
    }
}