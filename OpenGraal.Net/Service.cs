using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenGraal.Net;

public sealed class Service<TProtocol> : BackgroundService, IServiceEvents
    where TProtocol : IProtocol
{
    private readonly ILogger<Service<TProtocol>> _logger;
    private readonly ConnectionManager<TProtocol> _connectionManager;
    private readonly string _name;
    private readonly ServiceOptions _options = new();
    private Socket? _socket;

    public Service(
        ILogger<Service<TProtocol>> logger,
        IConfiguration configuration,
        ConnectionManager<TProtocol> connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _name = typeof(TProtocol).Name[..^8];

        configuration.GetSection(_name).Bind(_options);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, _options.Port));
        _socket.Listen((int) SocketOptionName.MaxConnections);

        _logger.LogInformation(
            "{Protocol} service started on port {Port}",
            _name, _options.Port);

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_socket is null)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await _socket.AcceptAsync(stoppingToken);

            _connectionManager.Create(this, client);
        }

        _logger.LogInformation("Server has stopped");
    }

    public void OnConnected(IConnection connection)
    {
        _logger.LogTrace(
            "[{SessionId}] {Address} has connected",
            connection.Id, connection.Address);
    }

    public void OnDisconnected(IConnection connection)
    {
        _logger.LogTrace(
            "[{SessionId}] {Address} has disconnected",
            connection.Id, connection.Address);

        _connectionManager.Destroy(connection.Id);
    }

    public void OnSocketError(IConnection connection, SocketError socketError)
    {
        _logger.LogWarning(
            "[{SessionId}] {Address} socket error {ErrorCode}",
            connection.Id, connection.Address, socketError);
    }
}