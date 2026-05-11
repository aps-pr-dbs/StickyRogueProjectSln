

namespace StickyRogueProject.Models;

// ─────────────────────────────────────────────────────────────
//  ItemType — ประเภทของ item
// ─────────────────────────────────────────────────────────────
public enum ItemType
{
    Consumable,   // ใช้แล้วหมดไป (Potion)
    Weapon,       // equip ได้ → เพิ่ม ATK
    Armor,        // equip ได้ → เพิ่ม DEF
    Accessory,    // equip ได้ → เพิ่ม stat พิเศษ
    Material,     // ของ drop ทั่วไป ยังใช้ไม่ได้
    QuestItem,    // ของ quest ยังใช้ไม่ได้
}

// ─────────────────────────────────────────────────────────────
//  InventoryItem — แทน string ด้วย object
// ─────────────────────────────────────────────────────────────
public class InventoryItem
{
    // ── Identity ──────────────────────────────────────────────
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎁";
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.Material;

    // ── Consumable effect ─────────────────────────────────────
    public int HpRestore { get; set; } = 0;
    public int MpRestore { get; set; } = 0;

    // ── Equipment stats ───────────────────────────────────────
    public int BonusAtk { get; set; } = 0;
    public int BonusDef { get; set; } = 0;
    public int BonusMagic { get; set; } = 0;
    public int BonusMaxHp { get; set; } = 0;
    public int BonusMaxMp { get; set; } = 0;

    // ── Helpers ───────────────────────────────────────────────
    public bool IsUsable => Type == ItemType.Consumable;
    public bool IsEquipment => Type is ItemType.Weapon or ItemType.Armor or ItemType.Accessory;

    /// <summary>สร้างจากชื่อผ่าน ItemRegistry (backward compat กับ List&lt;string&gt; เดิม)</summary>
    public static InventoryItem FromString(string name)
        => GameItem.Get(name) ?? new InventoryItem
        {
            Name = name,
            Icon = "🎁",
            Type = ItemType.Material,
            Description = "Unknown item"
        };

    public override string ToString() => Name;
}

// ─────────────────────────────────────────────────────────────
//  EquipmentSlots — slot อุปกรณ์ที่ตัวละคร equip อยู่
// ─────────────────────────────────────────────────────────────
public class EquipmentSlots
{
    public InventoryItem? Weapon { get; private set; }
    public InventoryItem? Armor { get; private set; }
    public InventoryItem? Accessory { get; private set; }

    // ── Bonus stats รวมจากทุก slot ───────────────────────────
    public int TotalBonusAtk => Sum(i => i.BonusAtk);
    public int TotalBonusDef => Sum(i => i.BonusDef);
    public int TotalBonusMagic => Sum(i => i.BonusMagic);
    public int TotalBonusMaxHp => Sum(i => i.BonusMaxHp);
    public int TotalBonusMaxMp => Sum(i => i.BonusMaxMp);

    private int Sum(Func<InventoryItem, int> selector)
        => (Weapon is not null ? selector(Weapon) : 0)
         + (Armor is not null ? selector(Armor) : 0)
         + (Accessory is not null ? selector(Accessory) : 0);

    // ── Equip → คืน item เก่าที่ถูกถอดออก (ใส่กลับ inventory) ──
    public InventoryItem? Equip(InventoryItem item)
    {
        InventoryItem? oldItem = null;

        switch (item.Type)
        {
            case ItemType.Weapon:
                oldItem = Weapon;
                Weapon = item;
                break;
            case ItemType.Armor:
                oldItem = Armor;
                Armor = item;
                break;
            case ItemType.Accessory:
                oldItem = Accessory;
                Accessory = item;
                break;
        }

        return oldItem;
    }

    // ── Unequip → คืน item ที่ถอด ────────────────────────────
    public InventoryItem? Unequip(ItemType slot)
    {
        InventoryItem? old = GetSlot(slot);
        if (old is null) return null;

        switch (slot)
        {
            case ItemType.Weapon: Weapon = null; break;
            case ItemType.Armor: Armor = null; break;
            case ItemType.Accessory: Accessory = null; break;
        }
        return old;
    }

    public InventoryItem? GetSlot(ItemType type) => type switch
    {
        ItemType.Weapon => Weapon,
        ItemType.Armor => Armor,
        ItemType.Accessory => Accessory,
        _ => null
    };

    private static InventoryItem? SwapSlot(ref InventoryItem? slot, InventoryItem newItem)
    {
        var old = slot;
        slot = newItem;
        return old;
    }
}