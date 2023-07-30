using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenGraal.Data;
using OpenGraal.Net;
using OpenGraal.Server.Game.Worlds;
using Serilog;

namespace OpenGraal.Server.Game.Players;

public sealed class Player
{
    private readonly World _world;
    private readonly GameUser _user;
    private WorldLevel? _level;
    private string _guild = string.Empty;

    public int Id => _user.Id;
    public string NickName { get; set; } = "default";
    public int MaxHp { get; set; } = 3;
    public float Hp { get; set; } = 3.0f;
    public int Gralats { get; set; }
    public int Arrows { get; set; } = 10;
    public int Bombs { get; set; } = 5;
    public int BombPower { get; set; } = 1;
    public int SwordPower { get; set; } = 1;
    public string SwordImage { get; set; } = "sword1.png";
    public int ShieldPower { get; set; } = 1;
    public string ShieldImage { get; set; } = "shield1.png";
    public int GlovePower { get; set; } = 1;
    public string Gani { get; set; } = "idle";
    public string[] GaniAttributes { get; }
    public string HeadImage { get; set; } = "head0.png";
    public string Chat { get; set; } = string.Empty;
    public byte[] Colors { get; set; } = new byte[5];
    public float X { get; set; } = 30;
    public int X2 { get; set; }
    public float Y { get; set; } = 30.5f;
    public int Y2 { get; set; }
    public float Z { get; set; }
    public int Z2 { get; set; }
    public int Sprite { get; set; } = 2;
    public PlayerStatus Status { get; set; } = PlayerStatus.Male | PlayerStatus.AllowWeapons;
    public int CarrySprite { get; set; } = -1;
    public string Level { get; set; } = string.Empty;
    public string HorseImage { get; set; } = string.Empty;
    public int HorseBushes { get; set; }
    public int CarryNpc { get; set; }
    public int Ap { get; set; } = 50;
    public int ApCounter { get; set; }
    public int Mp { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public long OnlineTime { get; set; }
    public int UdpPort { get; set; }
    public int AdditionalFlags { get; set; }
    public string AccountName { get; set; } = "admin";
    public string BodyImage { get; set; } = "body.png";
    public float Rating { get; set; } = 1500.0f;
    public float Deviation { get; set; } = 350.0f;
    public int AttachNpc { get; set; }
    public string Language { get; set; } = "English";
    public int StatusMessage { get; set; }
    public string OsType { get; set; } = "wind";
    public int GmapLevelX { get; set; }
    public int GmapLevelY { get; set; }
    public string CommunityName { get; set; } = string.Empty;
    public int Codepage { get; set; } = 1252;

    public Player(World world, GameUser user)
    {
        _world = world;
        _user = user;

        GaniAttributes = new string[30];

        for (var i = 0; i < 30; ++i)
        {
            GaniAttributes[i] = string.Empty;
        }

        Colors[0] = 2;
        Colors[1] = 0;
        Colors[2] = 10;
        Colors[3] = 4;
        Colors[4] = 18;
    }

    public void Send(Action<Packet> packet)
    {
        _user.Send(packet);
    }

    public void SendToLevel(Action<Packet> packet)
    {
        _level?.SendToAll(packet);
    }
    
    public void SendLink(Link link)
    {
        Send(packet => packet
            .WriteGChar(1)
            .WriteStr(link.ToString()));
    }

    public void SendSign(Sign sign)
    {
        Send(packet => packet
            .WriteGChar(5)
            .WriteGChar(sign.X)
            .WriteGChar(sign.Y)
            .WriteStr("")); // TODO: Encode sign content...
    }

    public void SendLevelName(string levelName)
    {
        Send(packet => packet
            .WriteGChar(PacketId.LevelName)
            .WriteStr(levelName));
    }

    public void SendPropertiesToSelf(params PlayerProperty[] properties)
    {
        Send(packet => packet
            .WriteGChar(PacketId.PlayerProperties)
            .Write(GetProperties(properties)));
    }

    public void SendPropertiesToLevel(params PlayerProperty[] properties)
    {
        _level?.SendTo(packet => packet
                .WriteGChar(PacketId.OtherPlayerProperties)
                .WriteGShort(Id)
                .Write(GetProperties(properties)),
            player => player != this);
    }

    public void SendPropertiesToAll(params PlayerProperty[] properties)
    {
        _world.SendTo(packet => packet
                .WriteGChar(PacketId.OtherPlayerProperties)
                .WriteGShort(Id)
                .Write(GetProperties(properties)),
            player => player != this);
    }

    public void SendPropertiesToAllAndSelf(params PlayerProperty[] properties)
    {
        _world.SendTo(packet => packet
                .WriteGChar(PacketId.OtherPlayerProperties)
                .WriteGShort(Id)
                .Write(GetProperties(properties)),
            player => player != this);
        
        Send(packet => packet
            .WriteGChar(PacketId.PlayerProperties)
            .Write(GetProperties(properties)));
    }

    public void SendPropertiesTo(Player other, params PlayerProperty[] properties)
    {
        other.Send(packet => packet
            .WriteGChar(PacketId.OtherPlayerProperties)
            .WriteGShort(Id)
            .Write(GetProperties(properties)));
    }

    public void SendIsLeader()
    {
        Send(packet => packet.WriteGChar(10));
    }

    public void SendPlayerWarp(float x, float y, string levelName)
    {
        Send(packet => packet
            .WriteGChar(14)
            .WriteGChar((int) (x * 2))
            .WriteGChar((int) (y * 2))
            .WriteStr(levelName));
    }

    public void SendLevelModTime(long modTime)
    {
        Send(packet => packet
            .WriteGChar(39)
            .WriteGInt5(modTime));
    }

    public void SendNewWorldTime()
    {
        Send(packet => packet
            .WriteGChar(42)
            .WriteGInt4(World
                .GetTime()));
    }

    public void SendRaw<T>(T[] data) where T : struct
    {
        var bytes = MemoryMarshal.AsBytes(data.AsSpan());
        var bytesLen = bytes.Length;

        Send(packet => packet
            .WriteGChar(PacketId.RawData)
            .WriteGInt(1 + bytesLen + 1));

        Send(packet => packet
            .WriteGChar(PacketId.BoardPacket)
            .WriteRaw(data));
    }

    public void SendActiveLevel(string levelName)
    {
        Send(packet => packet
            .WriteGChar(156)
            .WriteStr(levelName));
    }

    public void SendGhosts(bool ghosts)
    {
        // Tell the client if there are any ghost players in the level.
        // Graal Reborn doesn't support trial accounts so pass 0 (no ghosts) instead of 1 (ghosts present).
        Send(packet => packet
            .WriteGChar(174)
            .WriteGChar(ghosts ? 1 : 0));
    }

    public void Disconnect()
    {
        LeaveLevel();

        _user.Disconnect();
    }

    private void LeaveLevel()
    {
        if (_level is null)
        {
            return;
        }

        _level.Remove(this);
        _level = null;
    }

    public void Warp(WorldLevel level, float x, float y)
    {
        if (level == _level)
        {
            WarpTo(x, y);

            return;
        }

        LeaveLevel();

        _level = level;
        _level.Add(this);
    }

    public void WarpTo(float x, float y)
    {
        X = x;
        Y = y;

        SendPropertiesToAll(
            PlayerProperty.X,
            PlayerProperty.Y);
    }

    public void SetProperties(Packet packet)
    {
        var propertiesToLevel = new List<PlayerProperty>();
        var propertiesToAll = new List<PlayerProperty>();

        while (packet.BytesRead < packet.Length)
        {
            var property = (PlayerProperty) packet.ReadGChar();

            if (ShouldForwardToLevel(property))
            {
                propertiesToLevel.Add(property);
            }

            if (ShouldForwardToAll(property))
            {
                propertiesToAll.Add(property);
            }

            SetProperty(property, packet);
        }

        if (propertiesToLevel.Count > 0)
        {
            SendPropertiesToLevel(propertiesToLevel.ToArray());
        }

        if (propertiesToAll.Count > 0)
        {
            SendPropertiesToAll(propertiesToAll.ToArray());
        }
    }

    [Pure]
    private static bool ShouldForwardToLevel(PlayerProperty property)
    {
        if (PlayerPropertySet.InitOthers.Contains(property))
        {
            return true;
        }

        return property switch
        {
            PlayerProperty.AttachNpc => true,
            _ => false
        };
    }

    [Pure]
    private static bool ShouldForwardToAll(PlayerProperty property)
    {
        return property switch
        {
            PlayerProperty.MaxHp => true,
            PlayerProperty.NickName => true,
            PlayerProperty.HeadImage => true,
            PlayerProperty.StatusMessage => true,
            _ => false
        };
    }


    private void SetGuild(string guild)
    {
        _guild = guild;
    }

    private void SetNickName(string nickName)
    {
        nickName = nickName.Trim();

        if (nickName.Length > 0 && nickName[0] == '*')
        {
            nickName = nickName[1..];
        }

        if (nickName.Length > 0 && nickName[^1] == ')')
        {
            var pos = nickName.LastIndexOf('(');

            if (pos != -1)
            {
                var guild = nickName[(pos + 1)..^1].Trim();

                SetGuild(guild);

                nickName = nickName[..pos].Trim();
            }
        }

        if (string.IsNullOrEmpty(nickName))
        {
            nickName = "unknown";
        }

        if (nickName.Equals(AccountName, StringComparison.OrdinalIgnoreCase))
        {
            nickName = "*" + AccountName;
        }

        if (!string.IsNullOrEmpty(_guild))
        {
            nickName += " (" + _guild + ")";
        }

        if (nickName != NickName)
        {
            Log.Information(
                "{AccountName} changed nickname from {OldNickName} to {NewNickName}",
                AccountName, NickName, nickName);
        }
        
        NickName = nickName;
    }

    private void SetMaxHp(int maxHp)
    {
        // TODO: Clip to: settings->getInt("heartlimit", 3));

        maxHp = Math.Min(20, maxHp);

        MaxHp = maxHp;
    }

    private void SetHp(float hp)
    {
        if (Ap < 40 && hp > MaxHp)
        {
            return;
        }

        Hp = hp;
    }

    private void SetGralats(int gralats)
    {
        Gralats = Math.Min(gralats, 9999999);
    }

    private void SetSword(int swordPower, string swordImage)
    {
        SwordPower = swordPower;
        SwordImage = swordImage;

        // TODO: clip(sp, ((settings->getBool("healswords", false) == true) ? -(settings->getInt("swordlimit", 3)) : 0), settings->getInt("swordlimit", 3));
    }

    private void SetShield(int shieldPower, string shieldImage)
    {
        ShieldPower = shieldPower;
        ShieldImage = shieldImage;

        // TODO: clip(sp, 0, settings->getInt("shieldlimit", 3));
    }

    private void SetGani(string gani)
    {
        Gani = gani;

        if (Gani == "spin")
        {
            /* TODO:
            CString nPacket;
            nPacket >> (char)PLO_HITOBJECTS >> (short)id >> (char)swordPower;
            char hx = (char)((x + 1.5f) * 2);
            char hy = (char)((y + 2.0f) * 2);
            server->sendPacketToLevel(CString() << nPacket >> (char)(hx) >> (char)(hy - 4), 0, level, this);
            server->sendPacketToLevel(CString() << nPacket >> (char)(hx) >> (char)(hy + 4), 0, level, this);
            server->sendPacketToLevel(CString() << nPacket >> (char)(hx - 4) >> (char)(hy), 0, level, this);
            server->sendPacketToLevel(CString() << nPacket >> (char)(hx + 4) >> (char)(hy), 0, level, this);
             */
        }
    }

    private void SetHead(string headImage)
    {
        HeadImage = headImage;
    }

    private bool HandleChatCommand(string chat)
    {
        var tokens = chat.Split(' ');
        if (tokens.Length == 0)
        {
            return false;
        }

        var command = tokens[0].ToLowerInvariant();
        if (command == "setnick")
        {
            SetNickName(chat[8..].Trim());

            SendPropertiesToAllAndSelf(PlayerProperty.NickName);

            return true;
        }

        return false;
    }

    private void SetChat(string chat)
    {
        if (HandleChatCommand(chat))
        {
            return;
        }

        Chat = chat;
    }

    private void SetPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;

        X2 = (int) (X * 16);
        Y2 = (int) (Y * 16);
        Z2 = (int) (Z * 16);

        // TODO: Set last movement time...

        Status &= ~PlayerStatus.Paused;

        TestSigns();
    }

    private void SetSprite(int sprite)
    {
        Sprite = sprite;

        TestSigns();
    }

    private void TestSigns()
    {
        if (_level is null)
        {
            return;
        }

        if (Sprite % 4 != 0)
        {
            return;
        }

/* TODO:
        std::vector<TLevelSign*>* signs = level->getLevelSigns();
        for (std::vector<TLevelSign*>::iterator i = signs->begin(); i != signs->end(); ++i)
        {
            TLevelSign* sign = *i;
            float signLoc[] = {(float)sign->getX(), (float)sign->getY()};
            if (y == signLoc[1] && inrange(x, signLoc[0]-1.5f, signLoc[0]+0.5f))
            {
                sendPacket(CString() >> (char)PLO_SAY2 << sign->getUText().replaceAll("\n", "#b"));
            }
        }
 */
    }

    private void SetStatus(PlayerStatus status)
    {
        var oldStatus = Status;

        Status = status;

        var wasDead = oldStatus.HasFlag(PlayerStatus.Dead);
        var isDead = status.HasFlag(PlayerStatus.Dead);

        /* Did the player come back to life? */
        if (wasDead && !isDead)
        {
            Hp = Ap switch
            {
                < 20 => 3,
                < 50 => 5,
                _ => MaxHp
            };

            Hp = Math.Min(Hp, MaxHp);

            /* TODO:
                    power = clip((ap < 20 ? 3 : (ap < 40 ? 5 : maxPower)), 0.5f, maxPower);
                    selfBuff >> (char)PLPROP_CURPOWER >> (char)(power * 2.0f);
                    levelBuff >> (char)PLPROP_CURPOWER >> (char)(power * 2.0f);

                    if (level != 0 && level->getPlayer(0) == this)
                        sendPacket(CString() >> (char)PLO_ISLEADER);
             */
        }

        /* Did the player die? */
        if (!wasDead && isDead)
        {
            /* TODO:
                    if (level->isSparringZone() == false)
                    {
                        deaths++;
                        dropItemsOnDeath();
                    }

                    // If we are the leader and there are more players on the level, we want to remove
                    // ourself from the leader position and tell the new leader that they are the leader.
                    if (level->getPlayer(0) == this && level->getPlayer(1) != 0)
                    {
                        level->removePlayer(this);
                        level->addPlayer(this);
                        level->getPlayer(0)->sendPacket(CString() >> (char)PLO_ISLEADER);
                    }
             */
        }
    }

    private void SetProperty(PlayerProperty property, Packet packet)
    {
        switch (property)
        {
            case PlayerProperty.NickName:
                SetNickName(packet.ReadNStr());
                break;

            case PlayerProperty.MaxHp:
                SetMaxHp(packet.ReadGChar());
                break;

            case PlayerProperty.Hp:
                SetHp(packet.ReadGChar() / 2.0f);
                break;

            case PlayerProperty.Gralats:
                SetGralats(packet.ReadGInt());
                break;

            case PlayerProperty.Arrows:
                Arrows = Math.Min((int) packet.ReadGChar(), 99);
                break;

            case PlayerProperty.Bombs:
                Bombs = Math.Min((int) packet.ReadGChar(), 99);
                break;

            case PlayerProperty.GlovePower:
                GlovePower = Math.Min((int) packet.ReadGChar(), 3);
                break;

            case PlayerProperty.BombPower:
                BombPower = Math.Min((int) packet.ReadGChar(), 3);
                break;

            case PlayerProperty.SwordPowerAndImage:
            {
                var swordPower = packet.ReadGChar();
                if (swordPower <= 4)
                {
                    var swordImage = "sword" + swordPower + ".png";

                    SetSword(swordPower, swordImage);
                }
                else
                {
                    SetSword(swordPower - 30, packet.ReadNStr());
                }

                break;
            }

            case PlayerProperty.ShieldPowerAndImage:
            {
                var shieldPower = packet.ReadGChar();
                if (shieldPower <= 3)
                {
                    var shieldImage = "shield" + shieldPower + ".png";

                    SetShield(shieldPower, shieldImage);
                }
                else
                {
                    SetShield(shieldPower - 10, packet.ReadNStr());
                }

                break;
            }

            case PlayerProperty.Gani:
                SetGani(packet.ReadNStr());
                break;

            case PlayerProperty.HeadImage:
            {
                var len = packet.ReadGChar();
                if (len < 100)
                {
                    var headImage = "head" + len + ".png";

                    SetHead(headImage);
                }
                else
                {
                    SetHead(packet.ReadStr(len - 100));
                }

                break;
            }

            case PlayerProperty.Chat:
                SetChat(packet.ReadNStr());
                break;

            case PlayerProperty.Colors:
                for (var i = 0; i < Colors.Length; ++i)
                {
                    Colors[i] = packet.ReadGChar();
                }

                break;

            case PlayerProperty.Id:
                packet.ReadGShort();
                break;

            case PlayerProperty.X:
                var x = packet.ReadGChar() / 2.0f;
                SetPosition(x, Y, Z);
                break;

            case PlayerProperty.Y:
                var y = packet.ReadGChar() / 2.0f;
                SetPosition(X, y, Z);
                break;

            case PlayerProperty.Z:
                var z = packet.ReadGChar() / 2.0f - 50;
                SetPosition(X, Y, z);
                break;

            case PlayerProperty.Sprite:
                SetSprite(packet.ReadGChar());
                break;

            case PlayerProperty.Status:
                SetStatus((PlayerStatus) packet.ReadGChar());
                break;

            case PlayerProperty.CarrySprite:
                CarryNpc = packet.ReadGChar();
                break;

            case PlayerProperty.Level:
                Level = packet.ReadNStr();
                break;

            case PlayerProperty.HorseImage:
                HorseImage = packet.ReadNStr();
                break;

            case PlayerProperty.HorseBushes:
                HorseBushes = packet.ReadGChar();
                break;

            case PlayerProperty.EffectColors:
            {
                var len = packet.ReadGChar();
                if (len > 0)
                {
                    packet.ReadGInt4();
                }

                break;
            }

            case PlayerProperty.CarryNpc:
                CarryNpc = packet.ReadGInt();

                /* TODO:
                if (server->getSettings()->getBool("duplicatecanbecarried", false) == false)
                {
                    bool isOwner = true;
                    {
                        std::vector<TPlayer*>* playerList = server->getPlayerList();
                        for (std::vector<TPlayer*>::iterator i = playerList->begin(); i != playerList->end(); ++i)
                        {
                            TPlayer* other = *i;
                            if (other == this) continue;
                            if (other->getProp(PLPROP_CARRYNPC).readGUInt() == carryNpcId)
                            {
                                // Somebody else got this NPC first.  Force the player to throw his down
                                // and tell the player to remove the NPC from memory.
                                carryNpcId = 0;
                                isOwner = false;
                                sendPacket(CString() >> (char)PLO_PLAYERPROPS >> (char)PLPROP_CARRYNPC >> (int)0);
                                sendPacket(CString() >> (char)PLO_NPCDEL2 >> (char)level->getLevelName().length() << level->getLevelName() >> (int)carryNpcId);
                                server->sendPacketToLevel(CString() >> (char)PLO_OTHERPLPROPS >> (short)id >> (char)PLPROP_CARRYNPC >> (int)0, pmap, this);
                                break;
                            }
                        }
                    }
                    if (isOwner)
                    {
                        // We own this NPC now so remove it from the level and have everybody else delete it.
                        TNPC* npc = server->getNPC(carryNpcId);
                        level->removeNPC(npc);
                        server->sendPacketToAll(CString() >> (char)PLO_NPCDEL2 >> (char)level->getLevelName().length() << level->getLevelName() >> (int)carryNpcId);
                    }
                }
                 */
                break;

            case PlayerProperty.ApCounter:
                ApCounter = packet.ReadGShort();
                break;

            case PlayerProperty.Mp:
                Mp = Math.Min((int) packet.ReadGChar(), 100);
                break;

            case PlayerProperty.Kills:
                packet.ReadGInt();
                break;

            case PlayerProperty.Deaths:
                packet.ReadGInt();
                break;

            case PlayerProperty.OnlineTime:
                packet.ReadGInt();
                break;

            case PlayerProperty.IpAddr:
                packet.ReadGInt5();
                break;

            case PlayerProperty.UdpPort:
                UdpPort = packet.ReadGInt();
                break;

            case PlayerProperty.Alignment:
                Ap = Math.Min((int) packet.ReadGChar(), 100);
                break;

            case PlayerProperty.AdditionalFlags:
                AdditionalFlags = packet.ReadGChar();
                break;

            case PlayerProperty.AccountName:
                packet.ReadNStr();
                break;

            case PlayerProperty.BodyImage:
                BodyImage = packet.ReadNStr();
                break;

            case PlayerProperty.Rating:
                packet.ReadGInt();
                break;

            case PlayerProperty.AttachNpc:
                packet.ReadGChar();
                AttachNpc = packet.ReadGInt();
                break;

            // Simplifies login.
            // Manually send prop if you are leaving the level.
            // 1 = join level, 0 = leave level.
            case PlayerProperty.InLevel:
                packet.ReadGChar();
                break;

            case PlayerProperty.Disconnected:
                break;

            case PlayerProperty.Language:
                Language = packet.ReadNStr();
                break;

            case PlayerProperty.StatusMessage:
                StatusMessage = packet.ReadGChar();
                break;

            case PlayerProperty.OsType:
                OsType = packet.ReadNStr();
                break;

            case PlayerProperty.Codepage:
                Codepage = packet.ReadGInt();
                break;

            case PlayerProperty.X2:
            {
                var x2 = packet.ReadGShort();
                X2 = x2 >> 1;
                if ((x2 & 0x0001) == 0x0001) X2 = -X2;
                SetPosition(X2 / 16.0f, Y, Z);
                break;
            }

            case PlayerProperty.Y2:
            {
                var y2 = packet.ReadGShort();
                Y2 = y2 >> 1;
                if ((y2 & 0x0001) == 0x0001) Y2 = -Y2;
                SetPosition(X, Y2 / 16.0f, Z);
                break;
            }

            case PlayerProperty.Z2:
            {
                var z2 = packet.ReadGShort();
                Z2 = z2 >> 1;
                if ((z2 & 0x0001) == 0x0001) Z2 = -Z2;
                SetPosition(X, Y, Z2 / 16.0f);
                break;
            }

            case PlayerProperty.GmapLevelX:
                GmapLevelY = packet.ReadGChar();
                /* TODO:
                if (pmap)
                {
                    levelName = pmap->getLevelAt(gmaplevelx, gmaplevely);
                    leaveLevel();
                    setLevel(levelName, -1);
                }
                 */
                break;

            case PlayerProperty.GmapLevelY:
                GmapLevelX = packet.ReadGChar();
                /* TODO:
                if (pmap)
                {
                    levelName = pmap->getLevelAt(gmaplevelx, gmaplevely);
                    leaveLevel();
                    setLevel(levelName, -1);
                }
                 */
                break;

            case PlayerProperty.CommunityName:
                packet.ReadNStr();
                break;

            default:
                var index = GetGaniAttributeIndex(property);
                if (index != -1)
                {
                    GaniAttributes[index] = packet.ReadNStr();
                }

                break;
        }
    }

    public Action<Packet> GetProperties(params PlayerProperty[] properties)
    {
        return packet =>
        {
            foreach (var property in properties)
            {
                GetProperty(property, packet);
            }
        };
    }

    private void GetProperty(PlayerProperty property, Packet packet)
    {
        packet.WriteGChar((int) property);

        switch (property)
        {
            case PlayerProperty.NickName:
                packet.WriteNStr(NickName);
                break;

            case PlayerProperty.MaxHp:
                packet.WriteGChar(MaxHp);
                break;

            case PlayerProperty.Hp:
                packet.WriteGChar((int) (Hp * 2));
                break;

            case PlayerProperty.Gralats:
                packet.WriteGInt(Gralats);
                break;

            case PlayerProperty.Arrows:
                packet.WriteGChar(Arrows);
                break;

            case PlayerProperty.Bombs:
                packet.WriteGChar(Bombs);
                break;

            case PlayerProperty.GlovePower:
                packet.WriteGChar(GlovePower);
                break;

            case PlayerProperty.BombPower:
                packet.WriteGChar(BombPower);
                break;

            case PlayerProperty.SwordPowerAndImage:
                packet.WriteGChar(SwordPower + 30);
                packet.WriteNStr(SwordImage);
                break;

            case PlayerProperty.ShieldPowerAndImage:
                packet.WriteGChar(ShieldPower + 10);
                packet.WriteNStr(ShieldImage);
                break;

            case PlayerProperty.Gani:
                packet.WriteNStr(Gani);
                break;

            case PlayerProperty.HeadImage:
                var str = HeadImage.AsSpan();
                if (str.Length > 123)
                {
                    str = str[..123];
                }

                packet.WriteGChar(str.Length + 100);
                packet.WriteStr(str);
                break;

            case PlayerProperty.Chat:
                packet.WriteNStr(Chat);
                break;

            case PlayerProperty.Colors:
                foreach (var color in Colors)
                {
                    packet.WriteGChar(color);
                }

                break;

            case PlayerProperty.Id:
                packet.WriteGShort(Id);
                break;

            case PlayerProperty.X:
                packet.WriteGChar((int) (X * 2));
                break;

            case PlayerProperty.Y:
                packet.WriteGChar((int) (Y * 2));
                break;

            case PlayerProperty.Z:
                packet.WriteGChar((int) (Z + 0.5f) + 50);
                break;

            case PlayerProperty.Sprite:
                packet.WriteGChar(Sprite);
                break;

            case PlayerProperty.Status:
                packet.WriteGChar((int) Status);
                break;

            case PlayerProperty.CarrySprite:
                packet.WriteGChar(CarrySprite);
                break;

            case PlayerProperty.Level:
                packet.WriteNStr(Level);
                break;

            case PlayerProperty.HorseImage:
                packet.WriteNStr(HorseImage);
                break;

            case PlayerProperty.HorseBushes:
                packet.WriteGChar(HorseBushes);
                break;

            case PlayerProperty.EffectColors:
                packet.WriteGChar(0);
                break;

            case PlayerProperty.CarryNpc:
                packet.WriteGInt(CarryNpc);
                break;

            case PlayerProperty.ApCounter:
                packet.WriteGShort(ApCounter + 1);
                break;

            case PlayerProperty.Mp:
                packet.WriteGChar(Mp);
                break;

            case PlayerProperty.Kills:
                packet.WriteGInt(Kills);
                break;

            case PlayerProperty.Deaths:
                packet.WriteGInt(Deaths);
                break;

            case PlayerProperty.OnlineTime:
                packet.WriteGInt((int) OnlineTime);
                break;

            case PlayerProperty.IpAddr:
                packet.WriteGInt5(0);
                break;

            case PlayerProperty.UdpPort:
                packet.WriteGInt(UdpPort);
                break;

            case PlayerProperty.Alignment:
                packet.WriteGChar(Ap);
                break;

            case PlayerProperty.AdditionalFlags:
                packet.WriteGChar(AdditionalFlags);
                break;

            case PlayerProperty.AccountName:
                packet.WriteNStr(AccountName);
                break;

            case PlayerProperty.BodyImage:
                packet.WriteNStr(BodyImage);
                break;

            case PlayerProperty.Rating:
                var rating = (((int) Rating & 0xFFF) << 9) | ((int) Deviation & 0x1FF);
                packet.WriteGInt(rating);
                break;

            case PlayerProperty.AttachNpc:
                packet.WriteGChar(0);
                packet.WriteGInt(AttachNpc);
                break;

            // Simplifies login.
            // Manually send prop if you are leaving the level.
            // 1 = join level, 0 = leave level.
            case PlayerProperty.InLevel:
                packet.WriteGChar(1);
                break;

            case PlayerProperty.Disconnected:
                break;

            case PlayerProperty.Language:
                packet.WriteNStr(Language);
                break;

            case PlayerProperty.StatusMessage:
                packet.WriteGChar(StatusMessage);
                break;

            case PlayerProperty.OsType:
                packet.WriteNStr(OsType);
                break;

            case PlayerProperty.Codepage:
                packet.WriteGInt(Codepage);
                break;

            case PlayerProperty.X2:
                var x2 = Math.Abs(X2) << 1;
                if (X2 < 0) x2 |= 0x0001;
                packet.WriteGShort(x2);
                break;

            case PlayerProperty.Y2:
                var y2 = Math.Abs(Y2) << 1;
                if (Y2 < 0) y2 |= 0x0001;
                packet.WriteGShort(y2);
                break;

            case PlayerProperty.Z2:
                var z2 = Math.Abs(Z2) << 1;
                if (Z2 < 0) z2 |= 0x0001;
                packet.WriteGShort(z2);
                break;

            case PlayerProperty.GmapLevelX:
                packet.WriteGChar(GmapLevelX);
                break;

            case PlayerProperty.GmapLevelY:
                packet.WriteGChar(GmapLevelY);
                break;

            case PlayerProperty.CommunityName:
                packet.WriteNStr(CommunityName);
                break;

            default:
                var index = GetGaniAttributeIndex(property);
                if (index != -1)
                {
                    packet.WriteNStr(GaniAttributes[index]);
                }

                break;
        }
    }

    [Pure]
    private static int GetGaniAttributeIndex(PlayerProperty property)
    {
        return property switch
        {
            PlayerProperty.GaniAttribute1 => 0,
            PlayerProperty.GaniAttribute2 => 1,
            PlayerProperty.GaniAttribute3 => 2,
            PlayerProperty.GaniAttribute4 => 3,
            PlayerProperty.GaniAttribute5 => 4,
            PlayerProperty.GaniAttribute6 => 5,
            PlayerProperty.GaniAttribute7 => 6,
            PlayerProperty.GaniAttribute8 => 7,
            PlayerProperty.GaniAttribute9 => 8,
            PlayerProperty.GaniAttribute10 => 9,
            PlayerProperty.GaniAttribute11 => 10,
            PlayerProperty.GaniAttribute12 => 11,
            PlayerProperty.GaniAttribute13 => 12,
            PlayerProperty.GaniAttribute14 => 13,
            PlayerProperty.GaniAttribute15 => 14,
            PlayerProperty.GaniAttribute16 => 15,
            PlayerProperty.GaniAttribute17 => 16,
            PlayerProperty.GaniAttribute18 => 17,
            PlayerProperty.GaniAttribute19 => 18,
            PlayerProperty.GaniAttribute20 => 19,
            PlayerProperty.GaniAttribute21 => 20,
            PlayerProperty.GaniAttribute22 => 21,
            PlayerProperty.GaniAttribute23 => 22,
            PlayerProperty.GaniAttribute24 => 23,
            PlayerProperty.GaniAttribute25 => 24,
            PlayerProperty.GaniAttribute26 => 25,
            PlayerProperty.GaniAttribute27 => 26,
            PlayerProperty.GaniAttribute28 => 27,
            PlayerProperty.GaniAttribute29 => 28,
            PlayerProperty.GaniAttribute30 => 29,
            _ => -1
        };
    }
}