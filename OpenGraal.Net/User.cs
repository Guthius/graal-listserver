namespace OpenGraal.Net;

public abstract class User
{
    public Channel Channel { get; private set; } = default!;

    public int Id => Channel.Id;
    public string Address => Channel.Address;
    
    internal void SetChannel(Channel channel)
    {
        Channel = channel;
    }

    public void Send(IPacket packet)
    {
        Channel.Send(packet);
    }

    public void Disconnect()
    {
        Channel.Close();
    }
}