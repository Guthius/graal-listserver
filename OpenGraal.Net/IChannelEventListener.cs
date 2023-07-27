using System.Net.Sockets;

namespace OpenGraal.Net;

public interface IChannelEventListener
{
    void Connected(Channel channel);
    void Disconnected(Channel channel);
    void Packet(Channel channel, Packet packet);
    void Error(Channel channel, SocketError socketError);
}