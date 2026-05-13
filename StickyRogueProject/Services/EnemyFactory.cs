namespace StickyRogueProject.Models;

public static class EnemyFactory
{
    private static readonly string[] NormalImages =
    {
        "monster_01.png", "monster_02.png", "monster_03.png",
        "monster_04.png", "monster_05.png", "monster_06.png",
        "monster_07.png", "monster_08.png", "monster_09.png",
    };

    private const string BossImage = "boss.png";

    private static readonly string[] NormalNames =
    {
        "Slime", "Skeleton", "Goblin", "Vampire", "Witch",
        "Orc", "Dark Elf", "Stone Golem", "Shadow Wolf",
    };

    public static Enemy CreateNormalEnemy(int wave, int currentLoop)
    {
        var rng = new Random();

        int imgIndex = rng.Next(NormalImages.Length);
        string image = NormalImages[imgIndex];
        string name = NormalNames[imgIndex];

        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.20;
        int baseHp = (int)((40 + wave * 8) * loopMultiplier);
        int baseAtk = (int)((6 + wave * 2) * loopMultiplier);
        int baseDef = (int)((2 + wave) * loopMultiplier);
        int baseInt = (int)((3 + wave) * loopMultiplier);

        // ⚡ ระบบดรอปเหรียญแบบใหม่ ⚡
        // Wave 5 ดรอป 4-7, Wave ปกติ ดรอป 1-5
        int coinDrop = wave == 5 ? rng.Next(4, 8) : rng.Next(1, 6);

        // ⚡ สุ่มความต้านทาน (โอกาส 20% กันกายภาพ, 20% กันเวทย์, 60% ปกติ)
        string resType = "None";
        double resRoll = rng.NextDouble();
        if (resRoll < 0.2) resType = "Physical";
        else if (resRoll < 0.4) resType = "Magic";

        return new Enemy
        {
            Name = name,
            ImageSource = image,
            Level = wave + (currentLoop - 1) * 10,
            MaxHp = baseHp,
            CurrentHp = baseHp,
            Atk = baseAtk,
            Def = baseDef,
            Int = baseInt,
            XpReward = 10 + wave * 5 + (currentLoop - 1) * 15,
            CoinReward = coinDrop,
            ResistanceType = resType,
            NextIntent = "Attack", // ท่าเริ่มต้น
            IntentIcon = "🗡️"
        };
    }

    public static Enemy CreateBossEnemy(int currentLoop)
    {
        var rng = new Random();
        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.25;
        int baseHp = (int)(250 * loopMultiplier);
        int baseAtk = (int)(30 * loopMultiplier);
        int baseDef = (int)(15 * loopMultiplier);
        int baseInt = (int)(20 * loopMultiplier);

        // ⚡ บอสดรอปเหรียญ 7-12 ⚡
        int coinDrop = rng.Next(7, 13);

        return new Enemy
        {
            Name = $"Dark Lord  (Loop {currentLoop})",
            ImageSource = BossImage,
            Level = 10 + (currentLoop - 1) * 10,
            MaxHp = baseHp,
            CurrentHp = baseHp,
            Atk = baseAtk,
            Def = baseDef,
            Int = baseInt,
            XpReward = 100 + (currentLoop - 1) * 50,
            CoinReward = coinDrop,
            ResistanceType = "None", // บอสไม่กันดาเมจพิเศษ แต่เลือดเยอะมากแทน
            NextIntent = "Attack",
            IntentIcon = "🗡️"
        };
    }
}