using JetBrains.Annotations;

namespace OpenGraal.Data;

public static class Item
{
    public const int Invalid = -1;

    public static class Types
    {
        public const int GreenRupee = 0;
        public const int BlueRupee = 1;
        public const int RedRupee = 2;
        public const int Bombs = 3;
        public const int Darts = 4;
        public const int Heart = 5;
        public const int Glove1 = 6;
        public const int Bow = 7;
        public const int Bomb = 8;
        public const int Shield = 9;
        public const int Sword = 10;
        public const int FullHeart = 11;
        public const int SuperBomb = 12;
        public const int BattleAxe = 13;
        public const int GoldenSword = 14;
        public const int MirrorShield = 15;
        public const int Glove2 = 16;
        public const int LizardShield = 17;
        public const int LizardSword = 18;
        public const int GoldRupee = 19;
        public const int FireBall = 20;
        public const int FireBlast = 21;
        public const int NukeShot = 22;
        public const int JoltBomb = 23;
        public const int SpinAttack = 24;
    }
    
    [Pure]
    public static string GetName(int itemIndex)
    {
        return itemIndex switch
        {
            Types.GreenRupee => "greenrupee",
            Types.BlueRupee => "bluerupee",
            Types.RedRupee => "redrupee",
            Types.Bombs => "bombs",
            Types.Darts => "darts",
            Types.Heart => "heart",
            Types.Glove1 => "glove1",
            Types.Bow => "bow",
            Types.Bomb => "bomb",
            Types.Shield => "shield",
            Types.Sword => "sword",
            Types.FullHeart => "fullheart",
            Types.SuperBomb => "superbomb",
            Types.BattleAxe => "battleaxe",
            Types.GoldenSword => "goldensword",
            Types.MirrorShield => "mirrorshield",
            Types.Glove2 => "glove2",
            Types.LizardShield => "lizardshield",
            Types.LizardSword => "lizardsword",
            Types.GoldRupee => "goldrupee",
            Types.FireBall => "fireball",
            Types.FireBlast => "fireblast",
            Types.NukeShot => "nukeshot",
            Types.JoltBomb => "joltbomb",
            Types.SpinAttack => "spinattack",
            _ => ""
        };
    }
    
    public static int GetIndex(string itemName)
    {
        itemName = itemName.ToLowerInvariant();

        return itemName switch
        {
            "greenrupee" => Types.GreenRupee,
            "bluerupee" => Types.BlueRupee,
            "redrupee" => Types.RedRupee,
            "bombs" => Types.Bombs,
            "darts" => Types.Darts,
            "heart" => Types.Heart,
            "glove1" => Types.Glove1,
            "bow" => Types.Bow,
            "bomb" => Types.Bomb,
            "shield" => Types.Shield,
            "sword" => Types.Sword,
            "fullheart" => Types.FullHeart,
            "superbomb" => Types.SuperBomb,
            "battleaxe" => Types.BattleAxe,
            "goldensword" => Types.GoldenSword,
            "mirrorshield" => Types.MirrorShield,
            "glove2" => Types.Glove2,
            "lizardshield" => Types.LizardShield,
            "lizardsword" => Types.LizardSword,
            "goldrupee" => Types.GoldRupee,
            "fireball" => Types.FireBall,
            "fireblast" => Types.FireBlast,
            "nukeshot" => Types.NukeShot,
            "joltbomb" => Types.JoltBomb,
            "spinattack" => Types.SpinAttack,
            _ => Invalid
        };
    }
}