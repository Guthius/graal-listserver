using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record SetPlayerProperties(Packet Properties) : ICommand<SetPlayerProperties>
{
    public static SetPlayerProperties ReadFrom(Packet packet)
    {
        var bytes = packet.ReadBytes();
        var properties = new Packet();

        properties.SetBuffer(bytes, 0, bytes.Length);

        return new SetPlayerProperties(properties);
    }
}