namespace OpenGraal.Server.Game.Worlds;

public sealed class WorldOptions
{
    public int TickRate { get; set; }
    public string StartLevel { get; set; } = "onlinestartlocal.nw";
    public float StartX { get; set; } = 32;
    public float StartY { get; set; } = 25;
    public string UnstickmeLevel { get; set; } = "onlinestartlocal.nw";
    public float UnstickmeX { get; set; } = 36.5f;
    public float UnstickmeY { get; set; } = 33;
    public bool BushItems { get; set; } = true;
    public bool EnableDefaultWeapons { get; set; } = true;
}