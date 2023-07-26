using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Game.Packets;
using OpenGraal.Server.World;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.Game;

internal sealed class GameProtocol : Protocol
{
	private const uint IteratorStart = 0x04A80B38;
	
    private readonly ILogger<GameProtocol> _logger;
    private readonly IWorld _world;
    private uint _iterator = IteratorStart;
    private byte _key;
    private ClientType _clientType = ClientType.Await;
    private Player? _player;
    
    public GameProtocol(
        ILogger<GameProtocol> logger, 
        IWorld world) 
        : base(logger)
    {
        _logger = logger;
        _world = world;
        
        Bind<LanguagePacket>(37, OnLanguage);
    }
    
    private Memory<byte> RemoveInjectedByteFromPacket(Memory<byte> bytes)
    {
	    _iterator *= 0x8088405;
	    _iterator += _key;
	    
	    var pos = (int) (_iterator & 0x0FFFF) % bytes.Length;

	    bytes.Span[(pos + 1)..].CopyTo(bytes.Span[pos..]);

	    return bytes[..^1];
    }
    
    public override void Handle(IConnection connection, Memory<byte> bytes)
    {
	    /*
	     * First packet that arrives will always be the login packet. We need to handle this
	     * here because the login packet does not contain a packet ID.
	     */
	    if (_clientType == ClientType.Await)
	    {
		    HandleLogin(connection, bytes);
		    
		    return;
	    }

	    if (_player is null)
	    {
		    connection.Send(new DisconnectPacket(
			    "An unknown problem occured during your session."));
		    
		    connection.Disconnect();

		    return;
	    }

	    bytes = RemoveInjectedByteFromPacket(bytes);
	    
	    base.Handle(connection, bytes);
    }

    private void HandleLogin(IConnection connection,  Memory<byte> bytes)
    {
	    if (bytes.Length == 0)
	    {
		    return;
	    }

	    var byteSpan = bytes.Span;
	    
	    var clientType = (ClientType) (1 << (byteSpan[0] - 32));
	    
	    var stream = new PacketInputStream(bytes);
	    var packet = LoginPacket.ReadFrom(stream);

	    OnLogin(connection, clientType, packet);
    }
    
    private void OnLogin(IConnection connection, ClientType clientType, LoginPacket packet)
    {
	    _key = packet.Key;
	    _clientType = clientType;
	    
	    _logger.LogInformation("{AccountName} has logged in as {ClientType} ({ClientVersion})",
		    packet.AccountName, 
		    clientType,
		    packet.ClientVersion);
	    
	    // connection.Send(new DisconnectPacket(
		   //  "This server is currently restricted to staff only."));
	    //
	    // connection.Disconnect();

	    connection.Send(new SignaturePacket());
	    connection.Send(new Packet103());
	    connection.Send(new Packet194());
	    connection.Send(new Packet190());
	    connection.Send(new Packet168());

	    _player = _world.CreatePlayer(connection, packet.AccountName);
	    
	    if (_player is null)
	    {
		    connection.Send(new DisconnectPacket(
			    "This world is full"));
		    
		    connection.Disconnect();

		    return;
	    }
    }
    
    private void OnLanguage(IConnection connection, LanguagePacket packet)
    {
	    _world.SetLanguage(_player!, packet.Language);
    }
}