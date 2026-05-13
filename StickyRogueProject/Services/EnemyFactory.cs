namespace StickyRogueProject.Models;

public static class EnemyFactory
{
    private static readonly string[] NormalImages = { "monster_01.png", "monster_02.png", "monster_03.png", "monster_04.png", "monster_05.png", "monster_06.png", "monster_07.png", "monster_08.png", "monster_09.png" };
    private const string BossImage = "boss.png";

    private static readonly string[] NormalNames = { "Slime", "Skeleton", "Goblin", "Vampire", "Witch", "Orc", "Dark Elf", "Stone Golem", "Shadow Wolf" };

    // ⚡ คลังคำพูด (Dialogue Pool) ⚡
    private static readonly string[] BossQuotes = { "มีน้ำยาแค่นี้รึไอ้หนู?", "จงแหลกสลายไปซะ!", "แกไม่มีทางผ่าน Loop นี้ไปได้!", "อ่อนหัด!!", "คุกเข่าลงซะ!" };
    private static readonly string[] EnemyHitQuotes = { "ย๊ากกก!", "ตายซะ!", "เอาไปกิน!", "ฮี่ๆๆ โดนเต็มๆ", "เนื้อแกน่าอร่อยดีนะ!" };
    private static readonly string[] EnemyDodgeQuotes = { "ช้าไปไอ้หนู!", "มองไปทางไหนกัน?", "ฮ่าๆๆ วืดหรอ!", "กระจอก!", "ไม่ได้แอ้มหรอก!" };
    private static readonly string[] PlayerHitQuotes = { "ย๊ากก!!", "รับไปซะ!", "ตรงเป้า!", "ทะลวงเกราะ!", "อย่าอยู่เลยแก!" };
    private static readonly string[] PlayerDodgeQuotes = { "เชื่องช้าซะจริง!", "อ่านทางออกหมดแล้ว!", "ฮึบ! หลบได้!", "ไม่ได้กินฉันหรอก!" };

    public static string GetRandomEnemyAttackQuote(bool isBoss)
    {
        var rng = new Random();
        return isBoss ? BossQuotes[rng.Next(BossQuotes.Length)] : EnemyHitQuotes[rng.Next(EnemyHitQuotes.Length)];
    }
    public static string GetRandomEnemyDodgeQuote() => EnemyDodgeQuotes[new Random().Next(EnemyDodgeQuotes.Length)];
    public static string GetRandomPlayerAttackQuote() => PlayerHitQuotes[new Random().Next(PlayerHitQuotes.Length)];
    public static string GetRandomPlayerDodgeQuote() => PlayerDodgeQuotes[new Random().Next(PlayerDodgeQuotes.Length)];

    public static Enemy CreateNormalEnemy(int wave, int currentLoop)
    {
        var rng = new Random();
        int imgIndex = rng.Next(NormalImages.Length);
        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.20;
        int coinDrop = wave == 5 ? rng.Next(4, 8) : rng.Next(1, 6);
        string resType = "None";
        double resRoll = rng.NextDouble();
        if (resRoll < 0.2) resType = "Physical"; else if (resRoll < 0.4) resType = "Magic";

        return new Enemy
        {
            Name = NormalNames[imgIndex],
            ImageSource = NormalImages[imgIndex],
            Level = wave + (currentLoop - 1) * 10,
            MaxHp = (int)((40 + wave * 8) * loopMultiplier),
            CurrentHp = (int)((40 + wave * 8) * loopMultiplier),
            Atk = (int)((6 + wave * 2) * loopMultiplier),
            Def = (int)((2 + wave) * loopMultiplier),
            Int = (int)((3 + wave) * loopMultiplier),
            XpReward = 10 + wave * 5 + (currentLoop - 1) * 15,
            CoinReward = coinDrop,
            ResistanceType = resType,
            NextIntent = "Attack",
            IntentIcon = "🗡️"
        };
    }

    public static Enemy CreateBossEnemy(int currentLoop)
    {
        var rng = new Random();
        double loopMultiplier = 1.0 + (currentLoop - 1) * 0.25;
        return new Enemy
        {
            Name = $"Dark Lord  (Loop {currentLoop})",
            ImageSource = BossImage,
            Level = 10 + (currentLoop - 1) * 10,
            MaxHp = (int)(250 * loopMultiplier),
            CurrentHp = (int)(250 * loopMultiplier),
            Atk = (int)(30 * loopMultiplier),
            Def = (int)(15 * loopMultiplier),
            Int = (int)(20 * loopMultiplier),
            XpReward = 100 + (currentLoop - 1) * 50,
            CoinReward = rng.Next(7, 13),
            ResistanceType = "None",
            NextIntent = "Attack",
            IntentIcon = "🗡️"
        };
    }
}