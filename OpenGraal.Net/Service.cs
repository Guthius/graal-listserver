using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenGraal.Net;

public class Service<TUser, TParser> : BackgroundService 
    where TUser : User 
    where TParser : CommandParser<TUser>
{
    private readonly ILogger<Service<TUser, TParser>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<int> _sessionIds = new();
    private readonly Dictionary<int, Session> _sessions = new();
    private readonly string _name;
    private readonly ServiceOptions _options = new();
    private readonly CommandParser<TUser> _parser;
    private Socket? _socket;

    public Service(
        ILogger<Service<TUser, TParser>> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _name = typeof(TUser).Name[..^4];
        _parser = serviceProvider.GetRequiredService<TParser>();

        configuration.GetSection(_name).Bind(_options);

        _options.MaxConnections = Math.Max(1, _options.MaxConnections);

        // We reserve session ID's starting at ID 2.
        // ID's 0 and 1 are reserved for the server itself (0) and for the NPC-server (1).

        for (var i = _options.MaxConnections; i > 0; --i)
        {
            _sessionIds.Push(1 + i);
        }
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

            if (_sessionIds.Count == 0)
            {
                return;
            }

            var id = _sessionIds.Pop();
            var scope = _serviceProvider.CreateScope();
            var user = scope.ServiceProvider.GetRequiredService<TUser>();
            var session = new Session(this, scope, user);
            var channel = new Channel(id, session, user, client);

            user.SetChannel(channel);
            
            _sessions[id] = session;
            
            channel.Begin();
        }

        _logger.LogInformation("Server has stopped");
    }

    protected virtual void Connected(Channel channel, TUser user)
    {
        _logger.LogTrace(
            "[{SessionId}] {Address} has connected",
            channel.Id, channel.Address);
    }

    protected virtual void Disconnected(Channel channel, TUser user)
    {
        _logger.LogTrace(
            "[{SessionId}] {Address} has disconnected",
            channel.Id, channel.Address);

        if (!_sessions.TryGetValue(channel.Id, out var session))
        {
            return;
        }

        _sessions.Remove(channel.Id);
        _sessionIds.Push(channel.Id);

        session.Dispose();
    }

    protected virtual void HandlePacket(Channel channel, TUser user, Packet packet)
    {
        if (packet.Length == 0)
        {
            return;
        }
        
        _parser.Handle(user, packet);
    }

    protected virtual void Error(Channel channel, SocketError socketError)
    {
        _logger.LogWarning(
            "[{SessionId}] {Address} socket error {ErrorCode}",
            channel.Id, channel.Address, socketError);
    }
    
    private sealed class Session : IChannelEventListener, IDisposable
    {
        private readonly Service<TUser, TParser> _service;
        private readonly IServiceScope _scope;
        private readonly TUser _user;
        
        public Session(Service<TUser, TParser> service, IServiceScope scope, TUser user)
        {
            _service = service;
            _scope = scope;
            _user = user;
        }
        
        public void Connected(Channel channel)
        {
            _service.Connected(channel, _user);
        }

        public void Disconnected(Channel channel)
        {
            _service.Disconnected(channel, _user);
        }

        public void Packet(Channel channel, Packet packet)
        {
            _service.HandlePacket(channel, _user, packet);
        }

        public void Error(Channel channel, SocketError socketError)
        {
            _service.Error(channel, socketError);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}