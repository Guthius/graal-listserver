using OpenGraal.Net;
using OpenGraal.Server.Lobby.Dtos;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record ServerList(List<ServerInfo> ServerInfos) : IPacket
{
    private const int Id = 0;
    
    public void WriteTo(Packet packet)
    {
        packet.WriteGChar(Id);
        packet.WriteGChar(ServerInfos.Count);

        foreach (var serverInfo in ServerInfos)
        {
            
            var name = serverInfo.Name;
            if (serverInfo.Premium)
            {
                name = "P " + name;
            }
            
            packet.WriteGChar(8);
            packet.WriteNStr(name);
            packet.WriteNStr(serverInfo.Language);
            packet.WriteNStr(serverInfo.Description);
            packet.WriteNStr(serverInfo.Url);
            packet.WriteNStr(serverInfo.Version);
            packet.WriteNStr(serverInfo.Players.ToString());
            packet.WriteNStr(serverInfo.Ip);
            packet.WriteNStr(serverInfo.Port.ToString());
        }
    }
}