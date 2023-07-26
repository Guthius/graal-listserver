using JetBrains.Annotations;

namespace OpenGraal.Net;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IProtocol
{
    void Handle(IConnection connection, Memory<byte> bytes);
}