using OpenGraal.Net;
using OpenGraal.Server.Lobby.Dtos;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record ServerListPacket(List<ServerInfo> ServerInfos) : IServerPacket
{
    private const int Id = 0;

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