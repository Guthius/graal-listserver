﻿using OpenGraal.Net;

namespace OpenGraal.Server.Lobby.Packets;

internal sealed record PayByCreditCard(string Url) : IPacket
{
    private const int Id = 5;
    
    public void WriteTo(Packet packet)
    {
        packet.WriteGChar(Id);
        packet.WriteStr(Url);
    }
}