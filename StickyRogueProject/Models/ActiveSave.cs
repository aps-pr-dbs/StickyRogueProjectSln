using SQLite;
using System.Text.Json;
using System.Collections.Generic;

namespace StickyRogueProject.Models;

public class ActiveSave
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // ── ข้อมูลทั่วไป ──
    public string ClassName { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int CurrentWave { get; set; } = 1;
    public int CurrentLoop { get; set; } = 1;
    public int Coins { get; set; } = 0;

    // ── สเตตัสตัวละคร ──
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxMp { get; set; }
    public int CurrentMp { get; set; }
    public int Atk { get; set; }
    public int Def { get; set; }
    public int Int { get; set; }

    // ── Potions (Shortcut) ──
    public int HpPotionCount { get; set; }
    public int MpPotionCount { get; set; }

    // ── Shop Artifacts ── (👈 เพิ่มกลับเข้ามาสำหรับ ShopViewModel)
    public string ArtifactData { get; set; } = string.Empty;

    // =========================================================
    // ส่วน Inventory & Artifacts (แบบ List 6 ช่อง)
    // =========================================================

    // 1. Property สำหรับใช้งานในโค้ด
    [Ignore]
    public List<InventoryItem> Inventory { get; set; } = new();

    [Ignore]
    public List<InventoryItem> Artifacts { get; set; } = new();


    // 2. Property สำหรับ Save ลง SQLite (แปลง List เป็น JSON String)
    public string InventoryJson
    {
        get => JsonSerializer.Serialize(Inventory);
        set => Inventory = string.IsNullOrWhiteSpace(value)
            ? new List<InventoryItem>()
            : JsonSerializer.Deserialize<List<InventoryItem>>(value) ?? new List<InventoryItem>();
    }

    public string ArtifactsJson
    {
        get => JsonSerializer.Serialize(Artifacts);
        set => Artifacts = string.IsNullOrWhiteSpace(value)
            ? new List<InventoryItem>()
            : JsonSerializer.Deserialize<List<InventoryItem>>(value) ?? new List<InventoryItem>();
    }
}