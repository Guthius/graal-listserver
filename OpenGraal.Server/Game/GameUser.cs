using System.Diagnostics.CodeAnalysis;
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

    [MemberNotNullWhen(true, nameof(Player))]
    private bool IsAuthorized(bool disconnectIfNotAuthorized = true)
    {
        if (Player is not null)
        {
            return true;
        }

        if (disconnectIfNotAuthorized)
        {
            Disconnect("You are not authorized to perform this operation.");
        }

        return false;
    }

    public void Disconnect(string message)
    {
        Send(packet => packet
            .WriteGChar(PacketId.Disconnect)
            .WriteStr(message));

        Disconnect();
    }

    private void SendSignature()
    {
        Send(packet => packet
            .WriteGChar(PacketId.Signature)
            .WriteGChar(73));
    }

    public void SendFile(string fileName)
    {
        if (!IsAuthorized())
        {
            return;
        }
        
        var file = _world.FileManager.GetFile(fileName);
        if (file is null)
        {
            Send(packet => packet
                .WriteGChar(30)
                .WriteStr(fileName));

            return;
        }

        var fileData = File.ReadAllBytes(file.Path);
        if (fileData.Length == 0)
        {
            Send(packet => packet
                .WriteGChar(30)
                .WriteStr(fileName));

            return;
        }
        
        _logger.LogInformation("Sending {FileName} to {NickName} ({AccountName})",
            fileName, 
            Player.NickName, 
            Player.AccountName);

        if (fileData.Length > 3145728)
        {
            _logger.LogWarning(
                "Sending large file {FileName} (over 3MB)",
                fileName);
        }

        var largeFile = fileData.Length > 32000;

        if (largeFile)
        {
            Send(packet => packet
                .WriteGChar(PacketId.LargeFileStart)
                .WriteStr(fileName));

            Send(packet => packet
                .WriteGChar(PacketId.LargeFileSize)
                .WriteGInt5(fileData.Length));
        }

        var chunkSize = Math.Min(fileData.Length, 32000);
        var chunkHeaderSize = 1 + 5 + 1 + fileName.Length + 1;

        var bytesSent = 0;

        while (bytesSent < fileData.Length)
        {
            var count = Math.Min(fileData.Length - bytesSent, chunkSize);

            Send(packet => packet
                .WriteGChar(PacketId.RawData)
                .WriteGInt(chunkHeaderSize + count));

            var index = bytesSent;

            Send(packet => packet
                .WriteGChar(PacketId.File)
                .WriteGInt5(file.LastModified.ToUnixTimeSeconds())
                .WriteNStr(fileName)
                .WriteBytes(fileData, index, count));

            bytesSent += count;
        }

        if (largeFile)
        {
            Send(packet => packet
                .WriteGChar(PacketId.LargeFileEnd)
                .WriteStr(fileName));
        }
    }

    public void SendStartMessage()
    {
        var path = Path.Combine("World", "servermessage.html");
        if (!File.Exists(path))
        {
            return;
        }

        var message = File.ReadAllText(path);
        
        Send(packet => packet
            .WriteGChar(PacketId.StartMessage)
            .WriteStr(message));
    }

    public void SendStaffGuilds()
    {
        const string guilds = "Server,Manager,Owner,Admin,FAQ,LAT,NAT,GAT,GP,GP Chief,Bugs Admin,NPC Admin,Gani Team,GFX Admin,Events Team,Events Admin,Guild Admin";

        var str = string.Join(',', guilds
            .Split(',').Select(s => '"' + s + '"'));

        Send(packet => packet
            .WriteGChar(PacketId.StaffGuilds)
            .WriteStr(str));
    }

    public void SendRpgMessages(params string[] messages)
    {
        var message = string.Join(',', messages
            .Select(message =>
                '"' + message + '"'));

        Send(packet => packet
            .WriteGChar(PacketId.RpgWindow)
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

        SendStaffGuilds();
        SendStatuses();

        // TODO: Send flags...

        Send(packet => packet.WriteGChar(194));
        Send(packet => packet.WriteGChar(34).WriteStr("Bomb")); // Delete Weapon
        Send(packet => packet.WriteGChar(34).WriteStr("Bow")); // Delete Weapon
        Send(packet => packet.WriteGChar(190));

        var level = _world.GetLevel(_world.Options.StartLevel);
        if (level is null)
        {
            Disconnect("No starting level available on server");

            return;
        }

        Player.Warp(level, Player.X, Player.Y);

        // TODO: Send bigmap if it was set
        // TODO: Send the minimap if it was set...

        SendRpgMessages(
            "Welcome to OpenGraal!",
            "OpenGraal Server programmed by Guthius.");
        
        SendStartMessage();

        Send(packet => packet.WriteGChar(82)); // Enable Server Text
    }

    public void Warp(float x, float y, string levelName, DateTimeOffset? lastModified = null)
    {
        if (!IsAuthorized())
        {
            return;
        }

        var level = _world.GetLevel(levelName);
        if (level is null)
        {
            Player.WarpFailed(levelName);

            return;
        }

        Player.Warp(level, x, y, lastModified);
    }

    public void SetPlayerProperties(Packet properties)
    {
        if (!IsAuthorized())
        {
            return;
        }

        Player.SetProperties(properties);
    }

    public void ReportPlayerKiller(int killerId)
    {
        if (!IsAuthorized())
        {
            return;
        }

        var killer = _world.GetPlayer(killerId);
        if (killer is null)
        {
            return;
        }

        _logger.LogInformation(
            "{Victim} ({VictimAccountName}) was killed by {Killer} ({KillerAccountName})",
            Player.NickName,
            Player.AccountName,
            killer.NickName,
            killer.AccountName);
    }

    public void ShowImage(string fileName)
    {
        Player?.SendToLevel(packet => packet
            .WriteGChar(32)
            .WriteGShort(Id)
            .WriteStr(fileName));
    }

    public void UpdateFile(string fileName, DateTimeOffset lastModified)
    {
        if (!IsAuthorized())
        {
            return;
        }

        var file = _world.FileManager.GetFile(fileName);
        if (file is null)
        {
            return;
        }

        if (file.LastModified > lastModified)
        {
            // TODO: Send the updated file
        }
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

    public void TriggerAction(int npcId, float x, float y, string actionName)
    {
        if (Player is null)
        {
            return;
        }

        _logger.LogInformation(
            "{AccountName} triggered {ActionName} on NPC {NpcId} at ({X}, {Y})",
            Player.AccountName, actionName, npcId, x, y);

        Player.SendToLevel(packet => packet
            .WriteGChar(48)
            .WriteGShort(Id)
            .WriteGInt(npcId)
            .WriteGChar((byte) (x * 2))
            .WriteGChar((byte) (y * 2))
            .WriteStr(actionName));
    }

    public void Dispose()
    {
        if (Player is not null)
        {
            _world.DestroyPlayer(Player);
        }
    }
}