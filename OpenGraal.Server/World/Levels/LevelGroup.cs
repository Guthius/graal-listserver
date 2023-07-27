namespace OpenGraal.Server.World.Levels;

internal sealed class LevelGroup
{
    private readonly IWorld _world;

    public LevelGroup(IWorld world)
    {
        _world = world;
    }
}