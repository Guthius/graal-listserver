namespace OpenGraal.Server.Game.Players;

[Flags]
public enum PlayerStatus
{
    Paused = 0x01,
    Hidden = 0x02,
    Male = 0x04,
    Dead = 0x08,
    AllowWeapons = 0x10,
    HideSword = 0x20,
    HasSpin = 0x40,
}