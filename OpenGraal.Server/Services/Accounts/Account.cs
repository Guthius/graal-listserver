namespace OpenGraal.Server.Services.Accounts;

internal sealed record Account
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}