using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Game.Packets;
using OpenGraal.Server.World;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.Game;

internal sealed class GameUser : User
{
    private const uint IteratorStart = 0x04A80B38;
	
    private readonly ILogger<GameUser> _logger;
    private readonly IWorld _world;
    public ClientType ClientType = ClientType.Await;
    public uint Iterator = IteratorStart;
    public byte Key;
    public Player? Player;

    public GameUser(ILogger<GameUser> logger, IWorld world)
    {
        _logger = logger;
        _world = world;
    }

    public void Login(byte key, string accountName, string password, ClientType clientType, string clientVersion)
    {
        Key = key;
        ClientType = clientType;
	    
        _logger.LogInformation("{AccountName} has logged in as {ClientType} ({ClientVersion})",
            accountName, 
            clientType,
            clientVersion);
	    
        // connection.Send(new DisconnectPacket(
        //  "This server is currently restricted to staff only."));
        //
        // connection.Disconnect();

        Send(new SignaturePacket());
        Send(new Packet103());
        Send(new Packet194());
        Send(new Packet190());
        Send(new Packet168());

        Player = _world.CreatePlayer(this, accountName);
	    
        if (Player is null)
        {
            Send(new DisconnectPacket(
                "This world is full"));
		    
            Disconnect();

            return;
        }
    }
	
    public void SetLanguage(string language)
    {
        _world.SetLanguage(Player!, language);
    }
}