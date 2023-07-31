namespace OpenGraal.Server.Game.Files;

[Flags]
public enum FileCategory
{
    File = 1,
    Body = 2,
    Head = 4,
    Sword = 8,
    Shield = 16,
    Level = 32,

    Any = File | Body | Head | Sword | Shield | Level
}