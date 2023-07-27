using JetBrains.Annotations;

namespace OpenGraal.Net;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface ICommand<out TSelf>
{
    static abstract TSelf ReadFrom(Packet packet);
}