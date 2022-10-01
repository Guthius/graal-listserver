using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Listserver.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Listserver;

public class Server : BackgroundService
{
    private readonly IDatabase _database;
    private readonly ServerSettings _options;
    private Socket? _socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <param name="options">The server options.</param>
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

            _ = new Player(client, _database, _options).Run(stoppingToken);

            await Task.Delay(10, stoppingToken);
        }

        Log.Information("Server has stopped");
    }
}
