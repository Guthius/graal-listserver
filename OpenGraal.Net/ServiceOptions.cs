namespace OpenGraal.Net;

internal sealed record ServiceOptions
{
    public int Port { get; set; }
    public int MaxConnections { get; set; } = 100;
}