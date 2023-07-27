using OpenGraal.Net;
using OpenGraal.Server.Lobby.Dtos;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record ServerList(List<ServerInfo> ServerInfos) : IPacket
{
    private const int Id = 0;
    
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(Id);
        writer.WriteGChar(ServerInfos.Count);

        foreach (var serverInfo in ServerInfos)
        {
            
            var name = serverInfo.Name;
            if (serverInfo.Premium)
            {
                name = "P " + name;
            }
            
            writer.WriteGChar(8);
            writer.WriteNStr(name);
            writer.WriteNStr(serverInfo.Language);
            writer.WriteNStr(serverInfo.Description);
            writer.WriteNStr(serverInfo.Url);
            writer.WriteNStr(serverInfo.Version);
            writer.WriteNStr(serverInfo.Players.ToString());
            writer.WriteNStr(serverInfo.Ip);
            writer.WriteNStr(serverInfo.Port.ToString());
        }
    }
}