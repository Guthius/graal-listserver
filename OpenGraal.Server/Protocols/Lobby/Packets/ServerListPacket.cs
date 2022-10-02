using OpenGraal.Net;
using OpenGraal.Server.Services.Lobby;

namespace OpenGraal.Server.Protocols.Lobby.Packets;

internal sealed class ServerListPacket : IServerPacket
{
    private const int Id = 0;
    
    public List<ServerInfo> ServerInfos { get; set; } = new();
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteGChar(ServerInfos.Count);

        foreach (var serverInfo in ServerInfos)
        {
            
            var name = serverInfo.Name;
            if (serverInfo.Premium)
            {
                name = "P " + name;
            }
            
            output.WriteGChar(8);
            output.WriteNStr(name);
            output.WriteNStr(serverInfo.Language);
            output.WriteNStr(serverInfo.Description);
            output.WriteNStr(serverInfo.Url);
            output.WriteNStr(serverInfo.Version);
            output.WriteNStr(serverInfo.Players.ToString());
            output.WriteNStr(serverInfo.Ip);
            output.WriteNStr(serverInfo.Port.ToString());
        }
    }
}