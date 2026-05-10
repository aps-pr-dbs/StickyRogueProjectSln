namespace StickyRogueProject.Models;

public class Enemy
{
    // ชื่อศัตรูที่แสดงบน Combat Screen
    public string Name { get; set; } = string.Empty;

    // ชื่อไฟล์ภาพศัตรู เช่น "monster_01.png"
    // เก็บไว้ใน Resources/Images/ — ViewModel จะ Assign ให้ตาม Wave
    public string ImageSource { get; set; } = string.Empty;

    // Level ของศัตรู — ใช้แสดงบน UI เท่านั้น
    public int Level { get; set; } = 1;

    // HP ปัจจุบันของศัตรู
    public int CurrentHp { get; set; }

    // HP สูงสุดของศัตรู
    public int MaxHp { get; set; }

    // พลังโจมตีพื้นฐาน
    public int Atk { get; set; }

    // พลังป้องกัน — ใช้ลด Damage ที่ได้รับ
    public int Def { get; set; }

    // พลังเวทย์ — ใช้โจมตีแบบ Magic
    public int Int { get; set; }

    // XP ที่ได้เมื่อกำจัดศัตรูตัวนี้
    public int XpReward { get; set; }

    // เหรียญที่ได้เมื่อกำจัด
    public int CoinReward { get; set; }

    // ตรวจสอบว่าศัตรูตายแล้วหรือยัง
    public bool IsDefeated => CurrentHp <= 0;

    // Progress HP สำหรับ Binding กับ ProgressBar (0.0 - 1.0)
    public double HpProgress => MaxHp > 0
        ? Math.Clamp((double)CurrentHp / MaxHp, 0.0, 1.0)
        : 0.0;

    // ข้อความ HP สำหรับแสดงผล เช่น "45/100"
    public string HpText => $"{CurrentHp}/{MaxHp}";

    // รับ Damage โดยคำนวณ DEF ลดทอนก่อน
    // คืนค่า Damage จริงที่เกิดขึ้นหลัง DEF
    public int TakeDamage(int rawDamage)
    {
        int reduced = Math.Max(1, rawDamage - Def);
        CurrentHp = Math.Max(0, CurrentHp - reduced);
        return reduced;
    }

    // คำนวณ Damage ที่ศัตรูโจมตีผู้เล่น (Random ±20%)
    public int CalculateAttack()
    {
        var rng = new Random();
        double variation = 0.8 + rng.NextDouble() * 0.4; // 0.80 - 1.20
        return Math.Max(1, (int)(Atk * variation));
    }
}