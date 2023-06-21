using Serilog;

namespace OpenGraal.Data;

public sealed record Level(
    short[] Board, 
    List<Link> Links, 
    List<Baddy> Baddies, 
    List<Npc> Npcs, 
    List<Chest> Chests, 
    List<Sign> Signs)
{
    private const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    private const int BoardSize = 64 * 64;
    
    public static async Task<Level?> LoadNw(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);

        if (lines.Length == 0 || lines[0] != "GLEVNW01")
        {
            throw new Exception("Invalid level format");
        }

        var layerWarningLogged = false;
        var levelBoard = new short[BoardSize];
        var levelChests = new List<Chest>();
        var levelLinks = new List<Link>();
        var levelSigns = new List<Sign>();
        var levelNpcs = new List<Npc>();
        
        for (var i = 1; i < lines.Length; ++i)
        {
            var tokens = lines[i].Split(' ');
            if (tokens.Length == 0)
            {
                continue;
            }

            if (tokens[0] == "BOARD")
            {
                if (tokens.Length < 6)
                {
                    Log.Warning("Invalid board data in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                if (!int.TryParse(tokens[1], out var boardX) ||
                    !int.TryParse(tokens[2], out var boardY) ||
                    !int.TryParse(tokens[3], out var boardWidth) ||
                    !int.TryParse(tokens[4], out var boardLayer) ||
                    tokens[5].Length < boardWidth * 2)
                {
                    Log.Warning("Invalid board data in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                if (boardLayer > 0 && !layerWarningLogged)
                {
                    Log.Warning(
                        "Level {FileName} contains multiple layers. " +
                        "Layers are not supported at this time, only layer 0 has been loaded", 
                        path);
                    
                    layerWarningLogged = true;
                }
                
                for (var j = boardX; j < (boardX + boardWidth); ++j)
                {
                    var pos = (j - boardX) * 2;

                    var c1 = Base64.IndexOf(tokens[5][pos]);
                    var c2 = Base64.IndexOf(tokens[5][pos + 1]);

                    levelBoard[j + boardY*64] = (short)((c1 << 6) + c2);
                }
            }
            else if (tokens[0] == "CHEST")
            {
                if (tokens.Length < 5 ||
                    !int.TryParse(tokens[1], out var chestX) ||
                    !int.TryParse(tokens[2], out var chestY) ||
                    !int.TryParse(tokens[4], out var chestSignIndex))
                {
                    Log.Warning("Invalid chest in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                var chestItem = Item.GetIndexByName(tokens[3]);
                if (chestItem == -1)
                {
                    Log.Warning("Invalid item {ItemName} in chest in {FileName} on line {LineNumber}", 
                        tokens[3], path, i + 1);
                    
                    continue;
                }
                
                levelChests.Add(new Chest(chestX, chestY, chestItem, chestSignIndex));
            }
            else if (tokens[0] == "LINK")
            {
                if (tokens.Length < 7)
                {
                    Log.Warning("Invalid link in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }
                
                var linkLevel = tokens[1];
                
                var offset = 1;
                for (; offset < tokens.Length - 7; ++offset)
                {
                    linkLevel += " " + tokens[offset + 1];
                }

                if (!int.TryParse(tokens[offset + 1], out var linkX) ||
                    !int.TryParse(tokens[offset + 2], out var linkY) ||
                    !int.TryParse(tokens[offset + 3], out var linkWidth) ||
                    !int.TryParse(tokens[offset + 4], out var linkHeight))
                {
                    Log.Warning("Invalid link in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }
                
                levelLinks.Add(new Link(
                    linkLevel, 
                    linkX, 
                    linkY, 
                    linkWidth, 
                    linkHeight, 
                    tokens[offset + 5], 
                    tokens[offset + 6]));
            }
            else if (tokens[0] == "BADDY")
            {
                // TODO: Implement baddy load logic...
            }
            else if (tokens[0] == "SIGN")
            {
                if (tokens.Length < 3 || 
                    !int.TryParse(tokens[1], out var signX) ||
                    !int.TryParse(tokens[2], out var signY))
                {
                    Log.Warning("Invalid sign in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                var signText = string.Empty;

                while (++i < lines.Length)
                {
                    if (lines[i] == "SIGNEND")
                    {
                        break;
                    }

                    signText += lines[i] += "\n";
                }

                levelSigns.Add(new Sign(signX, signY, signText));
            }
            else if (tokens[0] == "NPC")
            {
                if (tokens.Length < 4)
                {
                    Log.Warning("Invalid NPC in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                var npcImage = tokens[1];

                var offset = 1;
                for (; offset < tokens.Length - 3; ++offset)
                {
                    npcImage += tokens[offset + 1];
                }

                if (!double.TryParse(tokens[offset + 1], out var npcX) ||
                    !double.TryParse(tokens[offset + 2], out var npcY))
                {
                    Log.Warning("Invalid NPC in {FileName} on line {LineNumber}", path, i + 1);
                    
                    continue;
                }

                var npcScript = string.Empty;
                
                while (++i < lines.Length)
                {
                    if (lines[i] == "NPCEND")
                    {
                        break;
                    }

                    npcScript += lines[i] + "\n";
                }

                levelNpcs.Add(new Npc(
                    npcX, 
                    npcY, 
                    npcImage,
                    npcScript));
            }
        }

        return new Level(
            levelBoard, 
            levelLinks,
            new List<Baddy>(), 
            levelNpcs,
            levelChests, 
            levelSigns);
    }
}