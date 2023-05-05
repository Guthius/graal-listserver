using System.Net.Sockets;

namespace OpenGraal.Net;

public interface IServiceEvents
{
    void OnConnected(IConnection connection);
    void OnDisconnected(IConnection connection);
    void OnSocketError(IConnection connection, SocketError socketError);
}