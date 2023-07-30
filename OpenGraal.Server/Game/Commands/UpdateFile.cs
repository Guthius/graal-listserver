using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record UpdateFile(long LastModified, string FileName) : ICommand<UpdateFile>
{
    public static UpdateFile ReadFrom(Packet packet)
    {
        return new UpdateFile(
            packet.ReadGInt5(), 
            packet.ReadStr());
    }
}