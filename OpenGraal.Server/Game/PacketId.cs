namespace OpenGraal.Server.Game;

public static class PacketId
{
    public const byte LevelName = 6;

    public const byte OtherPlayerProperties = 8;
    public const byte PlayerProperties = 9;

    public const byte Disconnect = 16;
    public const byte Signature = 25;
    public const byte StartMessage = 41;
    public const byte StaffGuilds = 47;

    public const byte LargeFileStart = 68;
    public const byte LargeFileEnd = 69;

    public const byte LargeFileSize = 84;
    
    public const byte RawData = 100;
    public const byte BoardPacket = 101;
    public const byte File = 102;

    public const byte RpgWindow = 179;
}