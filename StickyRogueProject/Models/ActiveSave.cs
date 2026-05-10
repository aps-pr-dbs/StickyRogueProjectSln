using SQLite;

namespace StickyRogueProject.Models;

[Table("ActiveSave")]
public class ActiveSave
{
    //ID อัตโนมัติ — ใช้เป็น Primary Key แต่ไม่ต้องสนใจมันมาก เพราะเราจะมีแค่ Record เดียวเท่านั้น
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // ---- ข้อมูลตัวละคร ----
    //ชื่อคลาสของตัวละคร (เช่น "Warrior", "Mage", "Rogue")
    [NotNull]
    public string ClassName { get; set; } = string.Empty;

    //Level ปัจจุบันของตัวละคร (เริ่มต้นที่ 1)
    [NotNull]
    public int Level { get; set; } = 1;

    // Stage ปัจจุบันที่ตัวละครอยู่ (เริ่มต้นที่ 1)
    [NotNull]
    public int CurrentStage { get; set; } = 1;

    // HP ปัจจุบันของตัวละคร — เริ่มต้นที่ 100 (หรือค่าที่เหมาะสมตามคลาส) และต้องไม่เกิน MaxHp
    [NotNull]
    public int CurrentHp { get; set; }

    // HP สูงสุดของตัวละคร
    [NotNull]
    public int MaxHp { get; set; }

    //จำนวนเหรียญที่มี — กฎสำคัญ: เริ่มต้นที่ 0 เสมอ
    [NotNull]
    public int Coins { get; set; } = 0;
    // ===== สเตตัสสะสมจาก Class + Artifact =====
    public int Atk { get; set; } = 0;
    public int Def { get; set; } = 0;
    public int Int { get; set; } = 0;
    // ===== ระบบ MP และ กระเป๋า Potion =====
    public int MaxMp { get; set; } = 0;
    public int CurrentMp { get; set; } = 0;
    
    public int HpPotionCount { get; set; } = 0;
    public int MpPotionCount { get; set; } = 0;

    // ===== ช่องเก็บข้อมูลระดับ Artifact ที่ซื้อไปแล้ว (เป็น JSON) =====
    public string ArtifactData { get; set; } = string.Empty;

    // ---- ระบบ Inventory (สูงสุด 6 ช่อง) ----
    // เก็บเป็น JSON String เพราะ SQLite ไม่รองรับ List<T> โดยตรง
    // ตัวอย่าง: "[\"Sword\",\"Shield\",\"\",\"\",\"\",\"\"]"

    //Inventory Slot 1 — ชื่อไอเทม หรือ "" ถ้าว่าง
    public string Slot1 { get; set; } = string.Empty;

    //Inventory Slot 2
    public string Slot2 { get; set; } = string.Empty;

    //Inventory Slot 3
    public string Slot3 { get; set; } = string.Empty;

    //Inventory Slot 4
    public string Slot4 { get; set; } = string.Empty;

    //Inventory Slot 5
    public string Slot5 { get; set; } = string.Empty;

    //Inventory Slot 6 — ช่องสุดท้าย (สูงสุด 6 ช่อง ตามกฎเกม)
    public string Slot6 { get; set; } = string.Empty;

    // ---- Timestamp ----

    //เวลาที่เริ่มเกมนี้ — บันทึกเป็น UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // เวลาที่ Save ล่าสุด
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;

    //---- Helper Method ----

    //คืนค่า List ของ Inventory ทั้ง 6 ช่อง สะดวกกว่าการเข้าถึง Slot1-Slot6 ทีละตัว

    [Ignore] // บอก SQLite ว่าไม่ต้องสร้าง Column นี้ในฐานข้อมูล
    public List<string> InventorySlots => new()
    {
        Slot1, Slot2, Slot3, Slot4, Slot5, Slot6
    };
    public int CurrentWave { get; set; } = 1;
    public int CurrentLoop { get; set; } = 1;
}