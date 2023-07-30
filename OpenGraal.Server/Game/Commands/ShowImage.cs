using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record ShowImage(string Data) : ICommand<ShowImage>
{
    public static ShowImage ReadFrom(Packet packet)
    {
        return new ShowImage(packet.ReadStr());
    }
}