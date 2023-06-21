namespace OpenGraal.Server.World.Players;




public enum PlayerEventType
{
    ItemPickup
}

public abstract class PlayerEvent
{
}

public sealed class Player
{
    public int Rupees { get; set; }
}