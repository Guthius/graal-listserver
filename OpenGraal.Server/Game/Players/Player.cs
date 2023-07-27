using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenGraal.Data;
using OpenGraal.Net;
using OpenGraal.Server.Game.Worlds;

namespace OpenGraal.Server.Game.Players;

public sealed class Player
{
    private readonly World _world;
    private readonly GameUser _user;
    private WorldLevel? _level;

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
    public string HeadImage { get; set; } = "head1.png";
    public string Chat { get; set; } = string.Empty;
    public byte[] Colors { get; set; } = new byte[5];
    public float X { get; set; } = 30;
    public int X2 { get; set; }
    public float Y { get; set; } = 30.5f;
    public int Y2 { get; set; }
    public float Z { get; set; }
    public int Z2 { get; set; }
    public int Sprite { get; set; } = 2;
    public int Status { get; set; } = 20;
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
    public string BodyImage { get; set; } = "body2.png";
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

    public void Send(IPacket packet)
    {
        _user.Send(packet);
    }

    public void Send(Action<Packet> packet)
    {
        _user.Send(packet);
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

    public void SendProperties(params PlayerProperty[] properties)
    {
        Send(GetProperties(properties));
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

        _world.SendTo(
            GetProperties(
                PlayerProperty.PLPROP_X,
                PlayerProperty.PLPROP_Y),
            player => player != this);
    }

    public Action<Packet> GetProperties(params PlayerProperty[] properties)
    {
        return packet =>
        {
            packet.WriteGChar(PacketId.PlayerProperties);

            foreach (var property in properties)
            {
                WriteProperty(packet, property);
            }
        };
    }

    private void WriteProperty(Packet packet, PlayerProperty property)
    {
        packet.WriteGChar((int) property);

        switch (property)
        {
            case PlayerProperty.PLPROP_NICKNAME:
                packet.WriteNStr(NickName);
                break;

            case PlayerProperty.PLPROP_MAXPOWER:
                packet.WriteGChar(MaxHp);
                break;

            case PlayerProperty.PLPROP_CURPOWER:
                packet.WriteGChar((int) (Hp * 2));
                break;

            case PlayerProperty.PLPROP_RUPEESCOUNT:
                packet.WriteGInt(Gralats);
                break;

            case PlayerProperty.PLPROP_ARROWSCOUNT:
                packet.WriteGChar(Arrows);
                break;

            case PlayerProperty.PLPROP_BOMBSCOUNT:
                packet.WriteGChar(Bombs);
                break;

            case PlayerProperty.PLPROP_GLOVEPOWER:
                packet.WriteGChar(GlovePower);
                break;

            case PlayerProperty.PLPROP_BOMBPOWER:
                packet.WriteGChar(BombPower);
                break;

            case PlayerProperty.PLPROP_SWORDPOWER:
                packet.WriteGChar(SwordPower + 30);
                packet.WriteNStr(SwordImage);
                break;

            case PlayerProperty.PLPROP_SHIELDPOWER:
                packet.WriteGChar(ShieldPower + 10);
                packet.WriteNStr(ShieldImage);
                break;

            case PlayerProperty.PLPROP_GANI:
                packet.WriteNStr(Gani);
                break;

            case PlayerProperty.PLPROP_HEADGIF:
                var str = HeadImage.AsSpan();
                if (str.Length > 123)
                {
                    str = str[..123];
                }

                packet.WriteGChar(str.Length + 100);
                packet.WriteStr(str);
                break;

            case PlayerProperty.PLPROP_CURCHAT:
                packet.WriteNStr(Chat);
                break;

            case PlayerProperty.PLPROP_COLORS:
                foreach (var color in Colors)
                {
                    packet.WriteGChar(color);
                }

                break;

            case PlayerProperty.PLPROP_ID:
                packet.WriteGShort(Id);
                break;

            case PlayerProperty.PLPROP_X:
                packet.WriteGChar((int) (X * 2));
                break;

            case PlayerProperty.PLPROP_Y:
                packet.WriteGChar((int) (Y * 2));
                break;

            case PlayerProperty.PLPROP_Z:
                packet.WriteGChar((int) (Z + 0.5f) + 50);
                break;

            case PlayerProperty.PLPROP_SPRITE:
                packet.WriteGChar(Sprite);
                break;

            case PlayerProperty.PLPROP_STATUS:
                packet.WriteGChar(Status);
                break;

            case PlayerProperty.PLPROP_CARRYSPRITE:
                packet.WriteGChar(CarrySprite);
                break;

            case PlayerProperty.PLPROP_CURLEVEL:
                packet.WriteNStr(Level);
                break;

            case PlayerProperty.PLPROP_HORSEGIF:
                packet.WriteNStr(HorseImage);
                break;

            case PlayerProperty.PLPROP_HORSEBUSHES:
                packet.WriteGChar(HorseBushes);
                break;

            case PlayerProperty.PLPROP_EFFECTCOLORS:
                packet.WriteGChar(0);
                break;

            case PlayerProperty.PLPROP_CARRYNPC:
                packet.WriteGInt(CarryNpc);
                break;

            case PlayerProperty.PLPROP_APCOUNTER:
                packet.WriteGShort(ApCounter + 1);
                break;

            case PlayerProperty.PLPROP_MAGICPOINTS:
                packet.WriteGChar(Mp);
                break;

            case PlayerProperty.PLPROP_KILLSCOUNT:
                packet.WriteGInt(Kills);
                break;

            case PlayerProperty.PLPROP_DEATHSCOUNT:
                packet.WriteGInt(Deaths);
                break;

            case PlayerProperty.PLPROP_ONLINESECS:
                packet.WriteGInt((int) OnlineTime);
                break;

            case PlayerProperty.PLPROP_IPADDR:
                packet.WriteGInt5(0);
                break;

            case PlayerProperty.PLPROP_UDPPORT:
                packet.WriteGInt(UdpPort);
                break;

            case PlayerProperty.PLPROP_ALIGNMENT:
                packet.WriteGChar(Ap);
                break;

            case PlayerProperty.PLPROP_ADDITFLAGS:
                packet.WriteGChar(AdditionalFlags);
                break;

            case PlayerProperty.PLPROP_ACCOUNTNAME:
                packet.WriteNStr(AccountName);
                break;

            case PlayerProperty.PLPROP_BODYIMG:
                packet.WriteNStr(BodyImage);
                break;

            case PlayerProperty.PLPROP_RATING:
                var rating = (((int) Rating & 0xFFF) << 9) | ((int) Deviation & 0x1FF);
                packet.WriteGInt(rating);
                break;

            case PlayerProperty.PLPROP_ATTACHNPC:
                packet.WriteGChar(0);
                packet.WriteGInt(AttachNpc);
                break;

            // Simplifies login.
            // Manually send prop if you are leaving the level.
            // 1 = join level, 0 = leave level.
            case PlayerProperty.PLPROP_JOINLEAVELVL:
                packet.WriteGChar(1);
                break;

            case PlayerProperty.PLPROP_PCONNECTED:
                break;

            case PlayerProperty.PLPROP_PLANGUAGE:
                packet.WriteNStr(Language);
                break;

            case PlayerProperty.PLPROP_PSTATUSMSG:
                packet.WriteGChar(StatusMessage);
                break;
            
            case PlayerProperty.PLPROP_OSTYPE:
                packet.WriteNStr(OsType);
                break;
            
            case PlayerProperty.PLPROP_TEXTCODEPAGE:
                packet.WriteGInt(Codepage);
                break;

            case PlayerProperty.PLPROP_X2:
                var x2 = Math.Abs(X2) << 1;
                if (X2 < 0) x2 |= 0x0001;
                packet.WriteGShort(x2);
                break;

            case PlayerProperty.PLPROP_Y2:
                var y2 = Math.Abs(Y2) << 1;
                if (Y2 < 0) y2 |= 0x0001;
                packet.WriteGShort(y2);
                break;

            case PlayerProperty.PLPROP_Z2:
                var z2 = Math.Abs(Z2) << 1;
                if (Z2 < 0) z2 |= 0x0001;
                packet.WriteGShort(z2);
                break;

            case PlayerProperty.PLPROP_GMAPLEVELX:
                packet.WriteGChar(GmapLevelX);
                break;

            case PlayerProperty.PLPROP_GMAPLEVELY:
                packet.WriteGChar(GmapLevelY);
                break;

            case PlayerProperty.PLPROP_COMMUNITYNAME:
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
            PlayerProperty.PLPROP_GATTRIB1 => 0,
            PlayerProperty.PLPROP_GATTRIB2 => 1,
            PlayerProperty.PLPROP_GATTRIB3 => 2,
            PlayerProperty.PLPROP_GATTRIB4 => 3,
            PlayerProperty.PLPROP_GATTRIB5 => 4,
            PlayerProperty.PLPROP_GATTRIB6 => 5,
            PlayerProperty.PLPROP_GATTRIB7 => 6,
            PlayerProperty.PLPROP_GATTRIB8 => 7,
            PlayerProperty.PLPROP_GATTRIB9 => 8,
            PlayerProperty.PLPROP_GATTRIB10 => 9,
            PlayerProperty.PLPROP_GATTRIB11 => 10,
            PlayerProperty.PLPROP_GATTRIB12 => 11,
            PlayerProperty.PLPROP_GATTRIB13 => 12,
            PlayerProperty.PLPROP_GATTRIB14 => 13,
            PlayerProperty.PLPROP_GATTRIB15 => 14,
            PlayerProperty.PLPROP_GATTRIB16 => 15,
            PlayerProperty.PLPROP_GATTRIB17 => 16,
            PlayerProperty.PLPROP_GATTRIB18 => 17,
            PlayerProperty.PLPROP_GATTRIB19 => 18,
            PlayerProperty.PLPROP_GATTRIB20 => 19,
            PlayerProperty.PLPROP_GATTRIB21 => 20,
            PlayerProperty.PLPROP_GATTRIB22 => 21,
            PlayerProperty.PLPROP_GATTRIB23 => 22,
            PlayerProperty.PLPROP_GATTRIB24 => 23,
            PlayerProperty.PLPROP_GATTRIB25 => 24,
            PlayerProperty.PLPROP_GATTRIB26 => 25,
            PlayerProperty.PLPROP_GATTRIB27 => 26,
            PlayerProperty.PLPROP_GATTRIB28 => 27,
            PlayerProperty.PLPROP_GATTRIB29 => 28,
            PlayerProperty.PLPROP_GATTRIB30 => 29,
            _ => -1
        };
    }
}