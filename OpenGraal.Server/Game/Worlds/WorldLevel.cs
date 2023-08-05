using OpenGraal.Data;
using OpenGraal.Net;
using OpenGraal.Server.Game.Players;

namespace OpenGraal.Server.Game.Worlds;

public sealed class WorldLevel
{
    private readonly Level _level;
    private readonly string _levelName;
    private readonly long _modTime;

    private readonly List<Player> _players = new();

    public World World { get; }
    public bool IsSparringZone { get; set; }

    public WorldLevel(World world, Level level, string levelName)
    {
        World = world;

        _level = level;
        _levelName = levelName;

        var fileInfo = new FileInfo(_levelName);

        _modTime = new DateTimeOffset(fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds();
    }

    public void Add(Player player)
    {
        lock (_players)
        {
            if (_players.Contains(player))
            {
                return;
            }

            _players.Add(player);
        }
        
        player.Level = _levelName;
        
        player.SendPropertiesToSelf(PlayerProperty.X, PlayerProperty.Y);
        player.SendPlayerWarp(player.X, player.Y, _levelName);
        player.SendLevelName(_levelName);
        player.SendRaw(_level.Board);

        foreach (var link in _level.Links)
        {
            player.SendLink(link);
        }

        foreach (var sign in _level.Signs)
        {
            player.SendSign(sign);
        }

        player.SendLevelModTime(_modTime);
        player.SendGhosts(false);
        player.SendIsLeader();
        player.SendNewWorldTime();
        player.SendActiveLevel(_levelName);

        lock (_players)
        {
            foreach (var other in _players)
            {
                if (other == player)
                {
                    continue;
                }
                
                player.SendPropertiesTo(other, 
                    PlayerPropertySet.Level);

                other.SendPropertiesTo(player, 
                    PlayerPropertySet.Level);
                
                player.Send(packet => packet
                    .WriteGChar(PacketId.OtherPlayerProperties)
                    .WriteGShort(other.Id)
                    .WriteGChar((byte) PlayerProperty.InLevel)
                    .WriteGChar(1));
                
                other.Send(packet => packet
                    .WriteGChar(PacketId.OtherPlayerProperties)
                    .WriteGShort(player.Id)
                    .WriteGChar((byte) PlayerProperty.InLevel)
                    .WriteGChar(1));
            }
        }
    }

    public void Remove(Player player)
    {
        lock (_players)
        {
            if (!_players.Remove(player))
            {
                return;
            }

            if (_players.Count == 0)
            {
                return;
            }

            _players[0].SendIsLeader();

            foreach (var other in _players)
            {
                player.Send(packet => packet
                    .WriteGChar(PacketId.OtherPlayerProperties)
                    .WriteGShort(other.Id)
                    .WriteGChar((int) PlayerProperty.InLevel)
                    .WriteGChar(0));
            }
        }

        SendToAll(packet => packet
            .WriteGChar(PacketId.OtherPlayerProperties)
            .WriteGShort(player.Id)
            .WriteGChar((int) PlayerProperty.InLevel)
            .WriteGChar(0));
    }

    public Player? GetPlayer(int index)
    {
        lock (_players)
        {
            if (index < 0 || index >= _players.Count)
            {
                return null;
            }

            return _players[index];
        }
    }

    public bool TrySwapPlayers(int index1, int index2)
    {
        lock (_players)
        {
            if (index1 < 0 || index1 >= _players.Count ||
                index2 < 0 || index2 >= _players.Count)
            {
                return false;
            }

            (_players[index1], _players[index2]) = (_players[index2], _players[index1]);
        }

        return true;
    }
    
    public void SendTo(Action<Packet> packet, Func<Player, bool> predicate)
    {
        lock (_players)
        {
            foreach (var player in _players.Where(predicate))
            {
                player.Send(packet);
            }
        }
    }

    public void SendToAll(Action<Packet> packet)
    {
        lock (_players)
        {
            foreach (var player in _players)
            {
                player.Send(packet);
            }
        }
    }
}