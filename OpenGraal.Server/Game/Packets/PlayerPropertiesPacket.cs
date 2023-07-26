using OpenGraal.Net;
using OpenGraal.Server.World.Players;

namespace OpenGraal.Server.Game.Packets;

public sealed record PlayerPropertiesPacket(Player Player, params PlayerProperty[] Properties) : IServerPacket
{
    private const int Id = 9;
	
    public void WriteTo(IPacketOutputStream output)
    {
        output.WriteGChar(Id);

        foreach (var property in Properties)
        {
            WriteProperty(property, output);
        }
    }

    private void WriteProperty(PlayerProperty property, IPacketOutputStream output)
    {
        output.WriteGChar((int) property);
        
        switch (property)
        {
			case PlayerProperty.PLPROP_NICKNAME:
                output.WriteNStr(Player.NickName);
                break;
            
            case PlayerProperty.PLPROP_MAXPOWER:
                output.WriteGChar(Player.MaxHp);
                break;
            
            case PlayerProperty.PLPROP_CURPOWER:
                output.WriteGChar((int) (Player.Hp * 2));
                break;
            
            case PlayerProperty.PLPROP_RUPEESCOUNT:
                output.WriteGInt(Player.Rupees);
                break;
            
            case PlayerProperty.PLPROP_ARROWSCOUNT:
                output.WriteGChar(Player.Arrows);
                break;
        }
    }
}