using Serilog;

namespace OpenGraal.Net;

public abstract class CommandParser<TUser> where TUser : User
{
    private readonly Dictionary<byte, Action<TUser, Packet>> _commands = new();
    
    protected void Bind<TCommand>(byte commandId, Action<TUser, TCommand> action) where TCommand : ICommand<TCommand>
    {
        _commands[commandId] = (user, packet) =>
        {
            var command = TCommand.ReadFrom(packet);

            action(user, command);
        };
    }

    public virtual void Handle(TUser user, Packet packet)
    {
        var commandId = packet.ReadGChar();

        if (!_commands.TryGetValue(commandId, out var commandHandler))
        {
            Log.Warning("Received unbound packet {CommandId:X2} ({Size} bytes) from {ClientAddress} [{ClientId}]",
                commandId, packet.Length, user.Address, user.Id);

            return;
        }

        Log.Verbose("Received {CommandId} packet ({Size} bytes) from {ClientAddress} [{ClientId}]",
            commandId, packet.Length, user.Address, user.Id);
        
        commandHandler(user, packet);
    }
}