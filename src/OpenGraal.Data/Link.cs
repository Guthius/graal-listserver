using System.Diagnostics.CodeAnalysis;

namespace OpenGraal.Data;

public sealed record Link(
    string FileName, 
    float X, 
    float Y, 
    float Width, 
    float Height, 
    string NewX, 
    string NewY)
{
    public static bool TryParse(string s, [NotNullWhen(true)] out Link? result)
    {
        result = null;
        
        var tokens = s.Split(' ');
        if (tokens.Length < 5)
        {
            return false;
        }

        var fileName = tokens[0];
        
        if (!float.TryParse(tokens[1], out var x) ||
            !float.TryParse(tokens[2], out var y) ||
            !float.TryParse(tokens[3], out var w) ||
            !float.TryParse(tokens[4], out var h))
        {
            return false;
        }

        var dx = "playerx";
        if (tokens.Length > 5)
        {
            dx = tokens[5];
            if (dx == "-1")
            {
                dx = "playerx";
            }
        }

        var dy = "playery";
        if (tokens.Length > 6)
        {
            dy = tokens[6];
            if (dy == "-1")
            {
                dy = "playery";
            }
        }

        result = new Link(fileName, x, y, w, h, dx, dy);

        return true;
    }

    public override string ToString()
    {
        return $"{FileName} {X} {Y} {Width} {Height} {NewY} {NewY}";
    }
}