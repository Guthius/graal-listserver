using JetBrains.Annotations;

namespace OpenGraal.Server.Services.Lobby;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class ServerInfo
{
    public bool Premium { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int Players { get; set; }
    public string Ip { get; set; } = string.Empty;
    public int Port { get; set; }
}