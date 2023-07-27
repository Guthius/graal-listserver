﻿using OpenGraal.Net;

namespace OpenGraal.Server.Game.Packets;

public sealed record DisconnectPacket(string Message) : IPacket
{
    private const int Id = 16;
    
    public void WriteTo(Packet writer)
    {
        writer.WriteGChar(Id);
        writer.WriteStr(Message);
    }
}