using JetBrains.Annotations;

namespace OpenGraal.Net;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface IClientPacket
{
    void ReadFrom(IPacketInputStream input);
}