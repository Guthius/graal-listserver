namespace OpenGraal.Net;

public interface IConnection : IDisposable
{
    int Id { get; }
    string Address { get; }
    void Send(IServerPacket packet);
    void Disconnect();
}