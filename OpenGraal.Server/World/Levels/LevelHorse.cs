namespace OpenGraal.Server.World.Levels;

public sealed record LevelHorse(int LifeTime, string Image, float X, float Y, char Direction, char Bushes);

public sealed record LevelChest(char X, char Y, LevelItemType ItemType, char SignIndex);




public sealed class Baddy
{
    public Baddy(float x, float y, char type)
    {
    }
}