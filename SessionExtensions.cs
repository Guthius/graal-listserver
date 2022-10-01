using System.Text;

namespace Listserver;

public static class SessionExtensions
{
    private static void Send(this ISession session, int type, string data)
    {
        var str = Convert.ToString((char)(type + 32)) + data + Convert.ToString((char)10);
        
        session.Send(Encoding.ASCII.GetBytes(str));
    }
    
    public static void ShowMessage(this ISession session, string message)
    {
        Send(session, Protocols.ServerList.Motd, message);
    }
    
    public static void SendServerList(this ISession session, string serverList)
    {
        Send(session, Protocols.ServerList.ServerListData, serverList);
    }

    public static void EnableShowMore(this ISession session, string url)
    {
        Send(session, Protocols.ServerList.ShowMore, url);
    }
    
    public static void EnablePayByCreditCard(this ISession session, string url)
    {
        Send(session, Protocols.ServerList.PayByCreditCard, url);
    }
    
    public static void EnablePayByPhone(this ISession session)
    {
        Send(session, Protocols.ServerList.PayByPhone, "1");
    }
}