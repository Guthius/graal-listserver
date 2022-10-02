namespace OpenGraal.Net;

public interface ISession : IDisposable
{
    int Id { get; }
    string Address { get; }
    void Send(IServerPacket packet);
    void Disconnect();
}