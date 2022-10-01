using System.Net.Sockets;

namespace Listserver;

public interface ISessionHandler
{
    void OnConnected(Session session);
    void OnDisconnected(Session session);
    void OnSocketError(Session session, SocketError socketError);
}