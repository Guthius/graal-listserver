using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record ReportPlayerKiller(int KillerId) : ICommand<ReportPlayerKiller>
{
    public static ReportPlayerKiller ReadFrom(Packet packet)
    {
        return new ReportPlayerKiller(packet.ReadGShort());
    }
}