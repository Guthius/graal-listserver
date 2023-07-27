using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

internal sealed record Login(
        byte Key,
        string ClientVersion,
        string AccountName,
        string Password) 
    : ICommand<Login>
{
    public static Login ReadFrom(Packet reader)
    {
        return new Login(
            reader.ReadGChar(),
            reader.ReadStr(8),
            reader.ReadNStr(),
            reader.ReadNStr());
    }
}