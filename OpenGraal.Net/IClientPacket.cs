using JetBrains.Annotations;

namespace OpenGraal.Net;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface IClientPacket<out TSelf>
{
    static abstract TSelf ReadFrom(IPacketInputStream input);
}