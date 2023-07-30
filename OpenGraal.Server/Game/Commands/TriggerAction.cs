using OpenGraal.Net;

namespace OpenGraal.Server.Game.Commands;

public sealed record TriggerAction(int NpcId, float X, float Y, string Action) : ICommand<TriggerAction>
{
    public static TriggerAction ReadFrom(Packet packet)
    {
        return new TriggerAction(
            packet.ReadGInt(), 
            packet.ReadGChar() / 2.0f, 
            packet.ReadGChar() / 2.0f, 
            packet.ReadStr());
    }
}