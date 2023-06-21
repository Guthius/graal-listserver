using JetBrains.Annotations;

namespace OpenGraal.Server.World.Levels;

public sealed record LevelItem(float X, float Y, LevelItemType Type)
{
    [Pure]
    public static int GetRupeeCount(LevelItemType type)
    {
        return type switch
        {
            LevelItemType.GREENRUPEE => 1,
            LevelItemType.BLUERUPEE => 5,
            LevelItemType.REDRUPEE => 30,
            LevelItemType.GOLDRUPEE => 100,
            _ => 0
        };
    }

    [Pure]
    public static bool IsRupeeType(LevelItemType type)
    {
        return GetRupeeCount(type) > 0;
    }
}