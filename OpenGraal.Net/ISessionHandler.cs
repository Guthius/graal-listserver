using System.Net.Sockets;

namespace OpenGraal.Net;

public interface ISessionHandler
{
    void OnConnected(ISession session);
    void OnDisconnected(ISession session);
    void OnSocketError(ISession session, SocketError socketError);
}