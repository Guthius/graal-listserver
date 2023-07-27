namespace OpenGraal.Server.Game;

[Flags]
public enum GameUserType
{
    Await = -1,
    
    Client = 1 << 0,
    Rc = 1 << 1,
    NpcServer = 1 << 2,
    Client2 = 1 << 4,
    Client3 = 1 << 5,
    Rc2 = 1 << 6,
    
    AnyClient = Client | Client2 | Client3,
    AnyRc = Rc | Rc2
}