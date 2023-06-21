namespace OpenGraal.Data;

public sealed record Baddy(
    int X,
    int Y, 
    int Type,
    string VerseSight,
    string VerseHurt,
    string VerseAttack);