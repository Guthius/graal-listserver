using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record GetMapInfo : ICommand<GetMapInfo>
{
    public static GetMapInfo ReadFrom(Packet packet)
    {
        return new GetMapInfo();
    }
}