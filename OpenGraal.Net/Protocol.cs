using Microsoft.Extensions.Logging;

namespace OpenGraal.Net;

public abstract class Protocol : IProtocol
{
    private readonly ILogger _logger;
    private readonly Dictionary<byte, Action<ISession, ReadOnlyMemory<byte>>> _actions = new();

    protected Protocol(ILogger logger)
    {
        _logger = logger;
    }

    protected void Bind<TPacket>(byte command, Action<ISession, TPacket> action)
        where TPacket : IClientPacket, new()
    {
        _actions[command] = (session, bytes) =>
        {
            var packet = ParsePacket<TPacket>(bytes);

            action(session, packet);
        };
    }

    private static TPacket ParsePacket<TPacket>(ReadOnlyMemory<byte> bytes) where TPacket : IClientPacket, new()
    {
        var stream = new PacketInputStream(bytes);

        var packet = new TPacket();

        packet.ReadFrom(stream);

        return packet;
    }

    public void Handle(ISession session, ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return;
        }

        var command = (byte)(bytes.Span[0] - 32);
        if (!_actions.TryGetValue(command, out var handler))
        {
            _logger.LogWarning("[{SessionId}] Received unhandled packet {Command} from '{Address}'", 
                session.Id, command, session.Address);
            
            return;
        }

        handler(session, bytes);
    }
}