namespace OpenGraal.Server.World.Players;

public enum PlayerProperty
{
	PLPROP_NICKNAME			= 0,
	PLPROP_MAXPOWER			= 1,
	PLPROP_CURPOWER			= 2,
	PLPROP_RUPEESCOUNT		= 3,
	PLPROP_ARROWSCOUNT		= 4,
	PLPROP_BOMBSCOUNT		= 5,
	PLPROP_GLOVEPOWER		= 6,
	PLPROP_BOMBPOWER		= 7,
	PLPROP_SWORDPOWER		= 8,
	PLPROP_SHIELDPOWER		= 9,
	PLPROP_GANI				= 10,	// PLPROP_BOWGIF in pre-2.x
	PLPROP_HEADGIF			= 11,
	PLPROP_CURCHAT			= 12,
	PLPROP_COLORS			= 13,
	PLPROP_ID				= 14,
	PLPROP_X				= 15,
	PLPROP_Y				= 16,
	PLPROP_SPRITE			= 17,
	PLPROP_STATUS			= 18,
	PLPROP_CARRYSPRITE		= 19,
	PLPROP_CURLEVEL			= 20,
	PLPROP_HORSEGIF			= 21,
	PLPROP_HORSEBUSHES		= 22,
	PLPROP_EFFECTCOLORS		= 23,
	PLPROP_CARRYNPC			= 24,
	PLPROP_APCOUNTER		= 25,
	PLPROP_MAGICPOINTS		= 26,
	PLPROP_KILLSCOUNT		= 27,
	PLPROP_DEATHSCOUNT		= 28,
	PLPROP_ONLINESECS		= 29,
	PLPROP_IPADDR			= 30,
	PLPROP_UDPPORT			= 31,
	PLPROP_ALIGNMENT		= 32,
	PLPROP_ADDITFLAGS		= 33,
	PLPROP_ACCOUNTNAME		= 34,
	PLPROP_BODYIMG			= 35,
	PLPROP_RATING			= 36,
	PLPROP_GATTRIB1			= 37,
	PLPROP_GATTRIB2			= 38,
	PLPROP_GATTRIB3			= 39,
	PLPROP_GATTRIB4			= 40,
	PLPROP_GATTRIB5			= 41,
	PLPROP_ATTACHNPC		= 42,
	PLPROP_GMAPLEVELX		= 43,
	PLPROP_GMAPLEVELY		= 44,
	PLPROP_Z				= 45,
	PLPROP_GATTRIB6			= 46,
	PLPROP_GATTRIB7			= 47,
	PLPROP_GATTRIB8			= 48,
	PLPROP_GATTRIB9			= 49,
	PLPROP_JOINLEAVELVL		= 50,
	PLPROP_PCONNECTED		= 51,
	PLPROP_PLANGUAGE		= 52,
	PLPROP_PSTATUSMSG		= 53,
	PLPROP_GATTRIB10		= 54,
	PLPROP_GATTRIB11		= 55,
	PLPROP_GATTRIB12		= 56,
	PLPROP_GATTRIB13		= 57,
	PLPROP_GATTRIB14		= 58,
	PLPROP_GATTRIB15		= 59,
	PLPROP_GATTRIB16		= 60,
	PLPROP_GATTRIB17		= 61,
	PLPROP_GATTRIB18		= 62,
	PLPROP_GATTRIB19		= 63,
	PLPROP_GATTRIB20		= 64,
	PLPROP_GATTRIB21		= 65,
	PLPROP_GATTRIB22		= 66,
	PLPROP_GATTRIB23		= 67,
	PLPROP_GATTRIB24		= 68,
	PLPROP_GATTRIB25		= 69,
	PLPROP_GATTRIB26		= 70,
	PLPROP_GATTRIB27		= 71,
	PLPROP_GATTRIB28		= 72,
	PLPROP_GATTRIB29		= 73,
	PLPROP_GATTRIB30		= 74,
	PLPROP_OSTYPE			= 75,	// 2.19+
	PLPROP_TEXTCODEPAGE		= 76,	// 2.19+
	PLPROP_UNKNOWN77		= 77,
	PLPROP_X2				= 78,
	PLPROP_Y2				= 79,
	PLPROP_Z2				= 80,
	PLPROP_UNKNOWN81		= 81,

	// In Graal v5, where players have the Graal######## accounts, this is their chosen account alias (community _name.)
	PLPROP_COMMUNITYNAME	= 82,
}

public sealed class Player
{
    public int Id { get; set; }
    public string NickName { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public float Hp { get; set; }
    public int Rupees { get; set; } // TODO: Rename to Gralats
    public int Arrows { get; set; }
    public int Bombs { get; set; }
    public int BombPower { get; set; }
    public int SwordPower { get; set; }
    public int ShieldPower { get; set; }
    public string Gani { get; set; } = string.Empty;
    public string HeadImage { get; set; } = string.Empty;
    public string Chat { get; set; } = string.Empty;
    public byte[] Colors { get; set; } = new byte[5];
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Sprite { get; set; }
    public int Status { get; set; }
    public int CarrySprite { get; set; }
    public string Level { get; set; } = string.Empty;
    public string HorseImage { get; set; } = string.Empty;
    public int HorseBushes { get; set; }
    public int CarryNpc { get; set; }
    public int Ap { get; set; }
    public int ApCounter { get; set; }
    public int Mp { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public long OnlineTime { get; set; }
    public int UdpPort { get; set; }
    public int AdditionalFlags { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string BodyImage { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int AttachNpc { get; set; }
    public string Language { get; set; } = string.Empty;
    public int StatusMessage { get; set; }
    public string OsType { get; set; } = string.Empty;
    public int GmapLevelX { get; set; }
    public int GmapLevelY { get; set; }
    public string CommunityName { get; set; } = string.Empty;
}