namespace StickyRogueProject.Models;

public static class EnemyFactory
{
    // รายชื่อไฟล์ภาพของ Normal Monster 9 ตัว
    // ผู้พัฒนากรอกชื่อไฟล์จริงตามที่ใส่ใน Resources/Images/
    private static readonly string[] NormalImages =
    {
        "monster_01.png",
        "monster_02.png",
        "monster_03.png",
        "monster_04.png",
        "monster_05.png",
        "monster_06.png",
        "monster_07.png",
        "monster_08.png",
        "monster_09.png",
    };

    // ไฟล์ภาพ Boss (Wave 10)
    private const string BossImage = "boss.png";

    // ชื่อ Random ของ Normal Monster (เลือกสุ่มตาม Wave)
    private static readonly string[] NormalNames =
    {
        "Slime",
        "Skeleton",
        "Goblin",
        "Bat",
        "Ghost",
        "Orc",
        "Dark Elf",
        "Stone Golem",
        "Shadow Wolf",
    };

    // ── CreateNormalEnemy ───────────────────────────────────────
    // สร้าง Normal Enemy สำหรับ Wave 1-9
    // wave       = Wave ปัจจุบัน (1-9) ใช้กำหนดความแข็งแกร่งในแต่ละ Loop
    // currentLoop = จำนวนรอบที่ผ่านมา (เริ่ม 1) ใช้ Scale Stats
    public static Enemy CreateNormalEnemy(int wave, int currentLoop)
    {
        var rng = new Random();

        // สุ่มภาพและชื่อ — แต่ละ Wave อาจเจอ Monster ต่างกัน
        int imgIndex = rng.Next(NormalImages.Length);
        string image = NormalImages[imgIndex];
        string name = NormalNames[imgIndex];

        // Base Stats ที่ Scale ตาม Wave และ Loop
        // ทุก Loop ที่เพิ่ม Stats จะเพิ่มขึ้น 20% เพื่อ Challenge ที่ไม่ซ้ำ
        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.20;
        int baseHp = (int)((40 + wave * 8) * loopMultiplier);
        int baseAtk = (int)((6 + wave * 2) * loopMultiplier);
        int baseDef = (int)((2 + wave) * loopMultiplier);
        int baseInt = (int)((3 + wave) * loopMultiplier);

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
            CoinReward = 5 + wave * 3 + (currentLoop - 1) * 10,
        };
    }

    // ── CreateBossEnemy ─────────────────────────────────────────
    // สร้าง Boss Enemy สำหรับ Wave 10
    // Stats สูงกว่า Normal Enemy ประมาณ 2.5 เท่าที่ Loop เดียวกัน
    public static Enemy CreateBossEnemy(int currentLoop)
    {
        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.25;
        int baseHp = (int)(250 * loopMultiplier);
        int baseAtk = (int)(30 * loopMultiplier);
        int baseDef = (int)(15 * loopMultiplier);
        int baseInt = (int)(20 * loopMultiplier);

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
            CoinReward = 50 + (currentLoop - 1) * 25,
        };
    }
}