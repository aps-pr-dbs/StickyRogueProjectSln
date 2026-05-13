namespace StickyRogueProject.Models;

public class Enemy
{
    // ชื่อศัตรูที่แสดงบน Combat Screen
    public string Name { get; set; } = string.Empty;

    // ชื่อไฟล์ภาพศัตรู
    public string ImageSource { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int Atk { get; set; }
    public int Def { get; set; }
    public int Int { get; set; }
    public int XpReward { get; set; }
    public int CoinReward { get; set; }

    // ⚡ ===== เพิ่มเติมสำหรับ Tactical Combat ===== ⚡

    // ประเภทต้านทาน: "None" (ปกติ), "Physical" (กัน ATK), "Magic" (กัน INT)
    public string ResistanceType { get; set; } = "None";

    // ท่าที่มอนสเตอร์จะใช้ในเทิร์นหน้า: "Attack", "Heavy", "Defend", "Magic"
    public string NextIntent { get; set; } = "Attack";

    // ไอคอนที่จะแสดงบน UI ให้ผู้เล่นเห็นล่วงหน้า
    public string IntentIcon { get; set; } = "🗡️";

    // ============================================

    public bool IsDefeated => CurrentHp <= 0;

    public double HpProgress => MaxHp > 0
        ? Math.Clamp((double)CurrentHp / MaxHp, 0.0, 1.0)
        : 0.0;

    public string HpText => $"{CurrentHp}/{MaxHp}";

    // (สมการ Damage ตอนนี้จะเป็นแค่พื้นฐาน ของจริงเราจะไปคำนวณใน CombatViewModel)
    public int TakeDamage(int rawDamage)
    {
        int reduced = Math.Max(1, rawDamage - Def);
        CurrentHp = Math.Max(0, CurrentHp - reduced);
        return reduced;
    }

    public int CalculateAttack()
    {
        var rng = new Random();
        double variation = 0.8 + rng.NextDouble() * 0.4;
        return Math.Max(1, (int)(Atk * variation));
    }
}