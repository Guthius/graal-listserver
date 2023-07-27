namespace OpenGraal.Data;

public sealed record Item
{
    public const int Invalid = -1;
    
    public static Item? GetByName(string itemName)
    {
        throw new NotImplementedException();
    }

    public static int GetIndexByName(string itemName)
    {
        itemName = itemName.ToLowerInvariant();

        return itemName switch
        {
            "greenrupee" => 0,
            "bluerupee" => 1,
            "redrupee" => 2,
            "bombs" => 3,
            "darts" => 4,
            "heart" => 5,
            "glove1" => 6,
            "bow" => 7,
            "bomb" => 8,
            "shield" => 9,
            "sword" => 10,
            "fullheart" => 11,
            "superbomb" => 12,
            "battleaxe" => 13,
            "goldensword" => 14,
            "mirrorshield" => 15,
            "glove2" => 16,
            "lizardshield" => 17,
            "lizardsword" => 18,
            "goldrupee" => 19,
            "fireball" => 20,
            "fireblast" => 21,
            "nukeshot" => 22,
            "joltbomb" => 23,
            "spinattack" => 24,
            _ => Invalid
        };
    }
}