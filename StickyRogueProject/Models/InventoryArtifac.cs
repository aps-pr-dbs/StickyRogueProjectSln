

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
public class InventoryArtifac
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
    public int BonusInt { get; set; } = 0;
    public int BonusMaxHp { get; set; } = 0;
    public int BonusMaxMp { get; set; } = 0;

    // ── Helpers ───────────────────────────────────────────────
    public bool IsUsable => Type == ItemType.Consumable;
    public bool IsEquipment => Type is ItemType.Weapon or ItemType.Armor or ItemType.Accessory;

    /// <summary>สร้างจากชื่อผ่าน ItemRegistry (backward compat กับ List&lt;string&gt; เดิม)</summary>
    /// <summary>สร้างจากชื่อผ่าน ArtifactRegistry (รองรับทั้ง Key และ Name)</summary>
    public static InventoryArtifac FromString(string keyOrName)
    {
        // 1. ลองค้นหาด้วย Key ก่อน (เช่น "atk_1")
        var artifact = Services.ArtifactRegistry.GetArtifact(keyOrName);

        // 2. ถ้าหาด้วย Key ไม่เจอ ให้ลองค้นหาจาก Name (เช่น "Catfood Hammer")
        if (artifact == null)
        {
            artifact = Services.ArtifactRegistry.GetAllArtifacts().FirstOrDefault(a => a.Name == keyOrName);
        }

        // 3. ถ้าหาเจอใน Registry ให้แปลงเป็น InventoryItem
        if (artifact != null)
        {
            var invItem = new InventoryArtifac
            {
                Name = artifact.Name,
                Icon = artifact.ImageSource,
                Description = artifact.Description,
                Type = artifact.StatType == "ATK" ? ItemType.Weapon : ItemType.Armor
            };

            switch (artifact.StatType)
            {
                case "ATK": invItem.BonusAtk = artifact.StatBonus; break;
                case "DEF": invItem.BonusDef = artifact.StatBonus; break;
                case "INT": invItem.BonusInt = artifact.StatBonus; break;
                case "HP": invItem.BonusMaxHp = artifact.StatBonus; break;
                case "MAXMP": invItem.BonusMaxMp = artifact.StatBonus; break;
            }
            return invItem;
        }

        // 4. ถ้าหาไม่เจอจริงๆ ให้คืนค่าเป็นไอเทมขยะ (Fallback)
        return new InventoryArtifac
        {
            Name = keyOrName,
            Icon = "🎁",
            Type = ItemType.Material,
            Description = "Unknown item"
        };
    }
    public override string ToString() => Name;
}

// ─────────────────────────────────────────────────────────────
//  EquipmentSlots — slot อุปกรณ์ที่ตัวละคร equip อยู่
// ─────────────────────────────────────────────────────────────
public class EquipmentSlots
{
    public InventoryArtifac? Weapon { get; private set; }
    public InventoryArtifac? Armor { get; private set; }
    public InventoryArtifac? Accessory { get; private set; }

    // ── Bonus stats รวมจากทุก slot ───────────────────────────
    public int TotalBonusAtk => Sum(i => i.BonusAtk);
    public int TotalBonusDef => Sum(i => i.BonusDef);
    public int TotalBonusInt => Sum(i => i.BonusInt);
    public int TotalBonusMaxHp => Sum(i => i.BonusMaxHp);
    public int TotalBonusMaxMp => Sum(i => i.BonusMaxMp);

    private int Sum(Func<InventoryArtifac, int> selector)
        => (Weapon is not null ? selector(Weapon) : 0)
         + (Armor is not null ? selector(Armor) : 0)
         + (Accessory is not null ? selector(Accessory) : 0);

    // ── Equip → คืน item เก่าที่ถูกถอดออก (ใส่กลับ inventory) ──
    public InventoryArtifac? Equip(InventoryArtifac item)
    {
        InventoryArtifac? oldItem = null;

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
    public InventoryArtifac? Unequip(ItemType slot)
    {
        InventoryArtifac? old = GetSlot(slot);
        if (old is null) return null;

        switch (slot)
        {
            case ItemType.Weapon: Weapon = null; break;
            case ItemType.Armor: Armor = null; break;
            case ItemType.Accessory: Accessory = null; break;
        }
        return old;
    }

    public InventoryArtifac? GetSlot(ItemType type) => type switch
    {
        ItemType.Weapon => Weapon,
        ItemType.Armor => Armor,
        ItemType.Accessory => Accessory,
        _ => null
    };

    private static InventoryArtifac? SwapSlot(ref InventoryArtifac? slot, InventoryArtifac newItem)
    {
        var old = slot;
        slot = newItem;
        return old;
    }
}