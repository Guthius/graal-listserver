using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

internal sealed record SetLanguage(string Language) : ICommand<SetLanguage>
{
    public static SetLanguage ReadFrom(Packet reader)
    {
        return new SetLanguage(reader.ReadStr());
    }
}