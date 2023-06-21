namespace OpenGraal.Server.World.Levels;

public sealed class LevelGroup
{
    private readonly IWorld _world;

    public LevelGroup(IWorld world)
    {
        _world = world;
    }
}