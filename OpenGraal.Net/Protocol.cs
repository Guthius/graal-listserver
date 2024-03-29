﻿using Microsoft.Extensions.Logging;

namespace OpenGraal.Net;

public abstract class Protocol(ILogger logger) : IProtocol
{
    private readonly Dictionary<byte, Action<IConnection, ReadOnlyMemory<byte>>> _actions = new();

    protected void Bind<TPacket>(byte command, Action<IConnection, TPacket> action)
        where TPacket : IClientPacket<TPacket>
    {
        _actions[command] = (session, bytes) =>
        {
            var stream = new PacketInputStream(bytes);

            var packet = TPacket.ReadFrom(stream);

            action(session, packet);
        };
    }

    public void Handle(IConnection connection, ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return;
        }

        var command = (byte) (bytes.Span[0] - 32);
        if (!_actions.TryGetValue(command, out var handler))
        {
            logger.LogWarning("[{SessionId}] Received unhandled packet {Command} from '{Address}'",
                connection.Id, command, connection.Address);

            return;
        }

        handler(connection, bytes);
    }
}