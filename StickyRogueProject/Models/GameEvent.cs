namespace StickyRogueProject.Models;

public enum EventEffectType
{
    None, GainCoins, LoseCoins, HealHalfHp, LoseHp, GainStat, LoseStat, DrugDealer
}

public class GameEvent
{
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public EventEffectType EffectType { get; set; }
    public int Value { get; set; }
    public string StatType { get; set; } = string.Empty;

    public bool IsDrugDealer => EffectType == EventEffectType.DrugDealer;
    public bool IsNormalEvent => !IsDrugDealer;
}

public static class EventPool
{
    private static readonly Random _rng = new();
    private static readonly string[] _statTypes = { "ATK", "INT", "DEF" };

    private static readonly (int caseId, int weight)[] _weights = new[]
    {
        (1, 5),   // lottery ticket (แพ้) — ธรรมดา
        (2, 2),   // lottery jackpot — หายาก
        (3, 8),   // onsen / heal — บ่อย
        (4, 8),   // rat bite / lose coins — บ่อย
        (5, 8),   // trap / lose HP — บ่อย
        (6, 6),   // gain stat tome — ค่อนข้างบ่อย
        (7, 6),   // lose stat tome — ค่อนข้างบ่อย
        (8, 3),   // old lady coins — ค่อนข้างหายาก
        (9, 2),   // drug dealer — หายาก
    };

    private static int RollWeighted()
    {
        int total = 0;
        foreach (var (_, w) in _weights) total += w;

        int roll = _rng.Next(total); // 0 .. total-1
        int cumulative = 0;
        foreach (var (caseId, w) in _weights)
        {
            cumulative += w;
            if (roll < cumulative) return caseId;
        }
        return _weights[^1].caseId; // fallback
    }

    public static GameEvent GetRandomEvent()
    {
        int roll = RollWeighted();
        var ev = new GameEvent();
        int xxx = 0;

        switch (roll)
        {
            case 1:
                ev.Title = "คุณเก็บหวยได้จากข้างทาง";
                ev.Story = "คุณเกลือ! หวังว่าวันต่อไปจะเป็นวันของคุณนะ";
                ev.ImagePath = "event_poor.png";
                ev.EffectType = EventEffectType.None;
                break;
            case 2:
                xxx = _rng.Next(1, 1001);
                ev.Title = "คุณเก็บหวยได้จากข้างทาง";
                ev.Story = $"คุณมือขึ้น! ถูกหวย, คุณได้รับเงินจำนวน {xxx} เหรียญ!";
                ev.ImagePath = "event_lottery.png";
                ev.EffectType = EventEffectType.GainCoins;
                ev.Value = xxx;
                break;
            case 3:
                ev.Title = "คุณเจอบ่อออนเซ็น";
                ev.Story = "ถึงเวลาพักผ่อน คุณฟื้นฟู 50% HP&MP";
                ev.ImagePath = "event_onsen.png";
                ev.EffectType = EventEffectType.HealHalfHp;
                break;
            case 4:
                xxx = _rng.Next(1, 201);
                ev.Title = "หนูกระโดดกัดกระเป๋าตังค์คุณ";
                ev.Story = $"คุณเสียเงินจำนวน {xxx} เหรียญ";
                ev.ImagePath = "event_rat.png";
                ev.EffectType = EventEffectType.LoseCoins;
                ev.Value = xxx;
                break;
            case 5:
                xxx = _rng.Next(1, 101);
                ev.Title = "คุณเหยียบกับดัก";
                ev.Story = $"โอ้ยยย! คุณเสีย {xxx} HP";
                ev.ImagePath = "event_trap.png";
                ev.EffectType = EventEffectType.LoseHp;
                ev.Value = xxx;
                break;
            case 6:
                ev.StatType = _statTypes[_rng.Next(_statTypes.Length)];
                ev.Title = "คุณอ่านหนังสือศักดิ์สิทธิ์";
                ev.Story = $"คุณได้รับ 3 {ev.StatType}.";
                ev.ImagePath = "event_tome.png";
                ev.EffectType = EventEffectType.GainStat;
                ev.Value = 5;
                break;
            case 7:
                ev.StatType = _statTypes[_rng.Next(_statTypes.Length)];
                ev.Title = "คุณอ่านหนังสือโบราณที่ไม่น่าไว้ใจ";
                ev.Story = $"คุณเสีย 3 {ev.StatType}";
                ev.ImagePath = "event_dtome.png";
                ev.EffectType = EventEffectType.LoseStat;
                ev.Value = 5;
                break;
            case 8:
                xxx = _rng.Next(1, 1001);
                ev.Title = "คุณเจอคุณป้าที่รวย";
                ev.Story = $"เธฮบอกคุณว่าคุณหน้าเหมือนหลาน เธอเลยให้เงินคุณจำนวน {xxx} เหรียญ";
                ev.ImagePath = "event_olady.png";
                ev.EffectType = EventEffectType.GainCoins;
                ev.Value = xxx;
                break;
            case 9:
                ev.Title = "คุณเจอคนขายยาที่หน้าสงสัย";
                ev.Story = "นี่คุณนะ, สนใจของดีมั้ย?";
                ev.ImagePath = "event_dealer2.png";
                ev.EffectType = EventEffectType.DrugDealer;
                break;
        }
        return ev;
    }
}