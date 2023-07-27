using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record GetFile(string FileName) : ICommand<GetFile>
{
    public static GetFile ReadFrom(Packet packet)
    {
        return new GetFile(packet.ReadStr());
    }
}