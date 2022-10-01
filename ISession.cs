namespace Listserver;

public interface ISession
{
    void Send(ReadOnlySpan<byte> bytes);
    void Disconnect();
}