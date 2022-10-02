using JetBrains.Annotations;

namespace OpenGraal.Net;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IProtocol
{
    void Handle(ISession session, ReadOnlyMemory<byte> bytes);
}