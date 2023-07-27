using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Game.Players;
using OpenGraal.Server.Game.Worlds;

namespace OpenGraal.Server.Game;

public sealed class GameUser : User, IDisposable
{
    private const uint IteratorStart = 0x04A80B38;

    private readonly ILogger<GameUser> _logger;
    private readonly World _world;
    
    public GameUserType ClientType = GameUserType.Await;
    public uint Iterator = IteratorStart;
    public byte Key;
    public Player? Player;
    
    public GameUser(ILogger<GameUser> logger, World world)
    {
        _logger = logger;
        _world = world;
    }

    public void Disconnect(string message)
    {
        Send(packet => packet
            .WriteGChar(16)
            .WriteStr(message));

        Disconnect();
    }

    private void SendSignature()
    {
        Send(packet => packet
            .WriteGChar(25)
            .WriteGChar(73));
    }
    
    public void SendFile(string fileName)
    {
        Send(packet => packet
            .WriteGChar(30)
            .WriteStr(fileName));
    }
    
    public void SendStartMessage(string message)
    {
        Send(packet => packet
            .WriteGChar(41)
            .WriteStr(message));
    }
    
    public void SendStaffGuilds()
    {
        const string guilds = "Server,Manager,Owner,Admin,FAQ,LAT,NAT,GAT,GP,GP Chief,Bugs Admin,NPC Admin,Gani Team,GFX Admin,Events Team,Events Admin,Guild Admin";

        var str = string.Join(',', guilds
            .Split(',').Select(s => '"' + s + '"'));
        
        Send(packet => packet
            .WriteGChar(47)
            .WriteStr(str));
    }
    
    public void SendRpgMessage(string message)
    {
        Send(packet => packet
            .WriteGChar(179)
            .WriteStr(message));
    }
    
    private void SendStatuses()
    {
        const string statuses = "Online,Away,DND,Eating,Hiding,No PMs,RPing,Sparring,PKing";

        var str = string.Join(',', statuses
            .Split(',').Select(s => '"' + s + '"'));

        Send(packet => packet
            .WriteGChar(180)
            .WriteStr(str));
    }
    
    public void Login(byte key, string accountName, string password, GameUserType clientType, string clientVersion)
    {
        Key = key;
        ClientType = clientType;

        _logger.LogInformation("{AccountName} has logged in as {ClientType} ({ClientVersion})",
            accountName,
            clientType,
            clientVersion);

        SendSignature();
        
        Send(packet => packet.WriteGChar(103).WriteStr(" *"));
        Send(packet => packet.WriteGChar(194));
        Send(packet => packet.WriteGChar(190));
        Send(packet => packet.WriteGChar(168));

        Player = _world.CreatePlayer(this, accountName);

        if (Player is null)
        {
            Disconnect("This world is full");

            return;
        }

        Send(Player.GetProperties(PlayerPropertySet.SendLogin));
        
        SendStaffGuilds();
        SendStatuses();

        // TODO: Send flags...

        Send(packet => packet.WriteGChar(194));
        Send(packet => packet.WriteGChar(34).WriteStr("Bomb")); // Delete Weapon
        Send(packet => packet.WriteGChar(34).WriteStr("Bow")); // Delete Weapon
        Send(packet => packet.WriteGChar(190));

        var level = _world.GetLevel("onlinestartlocal.nw");

        Player.Warp(level, Player.X, Player.Y);
        
        // TODO: Send bigmap if it was set
        // TODO: Send the minimap if it was set...

        SendRpgMessage(
            "\"Welcome to OpenGraal!\"," +
            "\"OpenGraal Server programmed by Guthius.\"");

        SendStartMessage("Hello World");
        
        Send(packet => packet.WriteGChar(82)); // Enable Server Text
    }
    
    public void SetLanguage(string language)
    {
        if (Player is null)
        {
            return;
        }

        Player.Language = language;
        
        _logger.LogInformation("{AccountName} set language to {Language}",
            Player.AccountName,
            Player.Language);
    }
    
    public void Dispose()
    {
        if (Player is not null)
        {
            _world.DestroyPlayer(Player);
        }
    }
}