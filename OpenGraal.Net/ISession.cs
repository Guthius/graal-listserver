namespace OpenGraal.Net;

public interface ISession
{
    string Ip { get; }
    void Send(IPacket packet);
    void Disconnect();
}