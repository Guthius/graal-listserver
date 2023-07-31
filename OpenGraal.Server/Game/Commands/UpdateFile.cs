using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record UpdateFile(DateTimeOffset LastModified, string FileName) : ICommand<UpdateFile>
{
    public static UpdateFile ReadFrom(Packet packet)
    {
        var lastModified = packet.ReadGInt5();
        var lastModifiedDate = DateTimeOffset.FromUnixTimeSeconds(lastModified);
        
        return new UpdateFile(
            lastModifiedDate, 
            packet.ReadStr());
    }
}