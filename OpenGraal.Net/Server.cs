using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenGraal.Net;

public sealed class Server<TProtocol> : BackgroundService, ISessionHandler where TProtocol : IProtocol
{
    private readonly SessionManager<TProtocol> _sessionManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Server<TProtocol>> _logger;
    private Socket? _socket;

    public Server(SessionManager<TProtocol> sessionManager, IConfiguration configuration,
        ILogger<Server<TProtocol>> logger)
    {
        _sessionManager = sessionManager;
        _configuration = configuration;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var port = _configuration.GetValue<int>("Port");

        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, port));
        _socket.Listen((int)SocketOptionName.MaxConnections);

        _logger.LogInformation("The server is running on port {Port}", port);
        _logger.LogInformation("Listening for connections...");

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Debug.Assert(_socket != null, nameof(_socket) + " != null");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await _socket.AcceptAsync(stoppingToken);

            var session = _sessionManager.Create(this, client);
            if (session is not null)
            {
                continue;
            }

            try
            {
                client.Close();
                client.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        _logger.LogInformation("Server has stopped");
    }

    public void OnConnected(ISession session)
    {
        _logger.LogInformation("[{SessionId}] {Address} has connected",
            session.Id, session.Address);
    }

    public void OnDisconnected(ISession session)
    {
        _logger.LogInformation("[{SessionId}] {Address} has disconnected",
            session.Id, session.Address);

        _sessionManager.Destroy(session.Id);
    }

    public void OnSocketError(ISession session, SocketError socketError)
    {
        _logger.LogWarning("[{SessionId}] {Address} socket error {ErrorCode}",
            session.Id, session.Address, socketError);
    }
}