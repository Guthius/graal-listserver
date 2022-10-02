using OpenGraal.Net;
using OpenGraal.Server.Database;

namespace OpenGraal.Server.Protocols.Lobby;

internal sealed class ServerListPacket : IServerPacket
{
    private const int Id = 0;
    
    private readonly List<ServerInfo> _serverInfos;
    
    public ServerListPacket(List<ServerInfo> serverInfos)
    {
        _serverInfos = serverInfos;
    }
    
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);
        output.WriteGChar(_serverInfos.Count);

        foreach (var serverInfo in _serverInfos)
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