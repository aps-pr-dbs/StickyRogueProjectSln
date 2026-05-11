
namespace StickyRogueProject.Models;

// ─────────────────────────────────────────────────────────────
//  ItemRegistry.cs
//  ฐานข้อมูลกลางของ item ทุกชิ้น
//  ใช้ชื่อเป็น key → ItemRegistry.Get("HP Potion")
//
//  เพิ่ม item ใหม่ที่นี่ที่เดียว ระบบทั้งหมดจะรู้จักอัตโนมัติ
// ─────────────────────────────────────────────────────────────
public static class GameItem
{
    private static readonly Dictionary<string, InventoryItem> _items = new()
    {
        // ══════════════════════════════════════════════════════
        //  CONSUMABLES — ใช้แล้วหมด
        // ══════════════════════════════════════════════════════

        ["Small HP Potion"] = new()
        {
            Name = "Small HP Potion",
            Icon = "🧪",
            Type = ItemType.Consumable,
            HpRestore = 30,
            Description = "Restores 30 HP",
        },
        ["HP Potion"] = new()
        {
            Name = "HP Potion",
            Icon = "🧪",
            Type = ItemType.Consumable,
            HpRestore = 60,
            Description = "Restores 60 HP",
        },
        ["Large HP Potion"] = new()
        {
            Name = "Large HP Potion",
            Icon = "🧪",
            Type = ItemType.Consumable,
            HpRestore = 120,
            Description = "Restores 120 HP",
        },
        ["Mana Potion"] = new()
        {
            Name = "Mana Potion",
            Icon = "💧",
            Type = ItemType.Consumable,
            MpRestore = 30,
            Description = "Restores 30 MP",
        },
        ["Large Mana Potion"] = new()
        {
            Name = "Large Mana Potion",
            Icon = "💧",
            Type = ItemType.Consumable,
            MpRestore = 80,
            Description = "Restores 80 MP",
        },

        // ══════════════════════════════════════════════════════
        //  WEAPONS — equip → Weapon slot
        // ══════════════════════════════════════════════════════

        ["Rusty Dagger"] = new()
        {
            Name = "Rusty Dagger",
            Icon = "🗡️",
            Type = ItemType.Weapon,
            BonusAtk = 5,
            Description = "+5 ATK",
        },
        ["Rusty Sword"] = new()
        {
            Name = "Rusty Sword",
            Icon = "⚔️",
            Type = ItemType.Weapon,
            BonusAtk = 8,
            Description = "+8 ATK",
        },
        ["Iron Club"] = new()
        {
            Name = "Iron Club",
            Icon = "🪵",
            Type = ItemType.Weapon,
            BonusAtk = 10,
            Description = "+10 ATK",
        },
        ["Legendary Sword"] = new()
        {
            Name = "Legendary Sword",
            Icon = "🗡️",
            Type = ItemType.Weapon,
            BonusAtk = 35,
            BonusMagic = 10,
            Description = "+35 ATK, +10 Magic",
        },

        // ══════════════════════════════════════════════════════
        //  ARMOR — equip → Armor slot
        // ══════════════════════════════════════════════════════

        ["Iron Shield"] = new()
        {
            Name = "Iron Shield",
            Icon = "🛡️",
            Type = ItemType.Armor,
            BonusDef = 8,
            Description = "+8 DEF",
        },
        ["Heavy Shield"] = new()
        {
            Name = "Heavy Shield",
            Icon = "🛡️",
            Type = ItemType.Armor,
            BonusDef = 15,
            BonusMaxHp = 20,
            Description = "+15 DEF, +20 Max HP",
        },
        ["Witch's Hat"] = new()
        {
            Name = "Witch's Hat",
            Icon = "🎩",
            Type = ItemType.Armor,
            BonusMagic = 12,
            BonusMaxMp = 20,
            Description = "+12 Magic, +20 Max MP",
        },
        ["Vampire Cape"] = new()
        {
            Name = "Vampire Cape",
            Icon = "🦇",
            Type = ItemType.Armor,
            BonusDef = 6,
            BonusMagic = 8,
            Description = "+6 DEF, +8 Magic",
        },
        ["Troll Hide"] = new()
        {
            Name = "Troll Hide",
            Icon = "🟫",
            Type = ItemType.Armor,
            BonusDef = 10,
            BonusMaxHp = 15,
            Description = "+10 DEF, +15 Max HP",
        },

        // ══════════════════════════════════════════════════════
        //  ACCESSORIES — equip → Accessory slot
        // ══════════════════════════════════════════════════════

        ["Wind Gem"] = new()
        {
            Name = "Wind Gem",
            Icon = "💨",
            Type = ItemType.Accessory,
            BonusAtk = 5,
            BonusMagic = 5,
            Description = "+5 ATK, +5 Magic",
        },
        ["Dark Gem"] = new()
        {
            Name = "Dark Gem",
            Icon = "🖤",
            Type = ItemType.Accessory,
            BonusMagic = 12,
            Description = "+12 Magic",
        },
        ["Earth Gem"] = new()
        {
            Name = "Earth Gem",
            Icon = "🟤",
            Type = ItemType.Accessory,
            BonusDef = 8,
            BonusMaxHp = 10,
            Description = "+8 DEF, +10 Max HP",
        },
        ["Fire Gem"] = new()
        {
            Name = "Fire Gem",
            Icon = "🔥",
            Type = ItemType.Accessory,
            BonusAtk = 10,
            BonusMagic = 10,
            Description = "+10 ATK, +10 Magic",
        },
        ["Dragon Shard"] = new()
        {
            Name = "Dragon Shard",
            Icon = "💎",
            Type = ItemType.Accessory,
            BonusAtk = 8,
            BonusDef = 5,
            BonusMagic = 8,
            Description = "+8 ATK, +5 DEF, +8 Magic",
        },

        // ══════════════════════════════════════════════════════
        //  MATERIALS — ใช้ไม่ได้ (ขายหรือ quest)
        // ══════════════════════════════════════════════════════

        ["Rat Tail"] = new() { Name = "Rat Tail", Icon = "🐭", Type = ItemType.Material, Description = "A gross rat tail. Sell it." },
        ["Goblin Ear"] = new() { Name = "Goblin Ear", Icon = "👂", Type = ItemType.Material, Description = "Proof of goblin slaying." },
        ["Orc Tusk"] = new() { Name = "Orc Tusk", Icon = "🦷", Type = ItemType.Material, Description = "A large orc tusk." },
        ["Bone Fragment"] = new() { Name = "Bone Fragment", Icon = "🦴", Type = ItemType.Material, Description = "Crumbled skeleton bone." },
        ["Ancient Scroll"] = new() { Name = "Ancient Scroll", Icon = "📜", Type = ItemType.Material, Description = "An old scroll. Mysterious." },
        ["Spell Scroll"] = new() { Name = "Spell Scroll", Icon = "📜", Type = ItemType.Material, Description = "Contains a forgotten spell." },
        ["Magic Dust"] = new() { Name = "Magic Dust", Icon = "✨", Type = ItemType.Material, Description = "Glittery magic powder." },
        ["Harpy Feather"] = new() { Name = "Harpy Feather", Icon = "🪶", Type = ItemType.Material, Description = "Soft yet sharp feather." },
        ["Blood Vial"] = new() { Name = "Blood Vial", Icon = "🩸", Type = ItemType.Material, Description = "Vampire blood, still warm." },
        ["Stone Core"] = new() { Name = "Stone Core", Icon = "🪨", Type = ItemType.Material, Description = "Golem's power source." },
        ["Iron Ore"] = new() { Name = "Iron Ore", Icon = "⛏️", Type = ItemType.Material, Description = "Raw iron, useful for crafting." },
        ["Wyvern Scale"] = new() { Name = "Wyvern Scale", Icon = "🐉", Type = ItemType.Material, Description = "Hard wyvern scale." },
        ["Wyvern Claw"] = new() { Name = "Wyvern Claw", Icon = "🦅", Type = ItemType.Material, Description = "Sharp wyvern claw." },
        ["Dragon Scale"] = new() { Name = "Dragon Scale", Icon = "🐲", Type = ItemType.Material, Description = "Extremely durable scale." },
        ["Dragon Claw"] = new() { Name = "Dragon Claw", Icon = "🐲", Type = ItemType.Material, Description = "Razor-sharp dragon claw." },
        ["Dragon Heart"] = new() { Name = "Dragon Heart", Icon = "❤️‍🔥", Type = ItemType.Material, Description = "Still burning with power." },
        ["Coin"] = new() { Name = "Coin", Icon = "🪙", Type = ItemType.Material, Description = "A small copper coin." },
        ["Gold Coin"] = new() { Name = "Gold Coin", Icon = "🪙", Type = ItemType.Material, Description = "Shiny gold coin." },
        ["Silver Coin"] = new() { Name = "Silver Coin", Icon = "🪙", Type = ItemType.Material, Description = "A silver coin." },
        ["Large Gold Coin"] = new() { Name = "Large Gold Coin", Icon = "🪙", Type = ItemType.Material, Description = "Worth quite a lot." },
    };

    // ── API ───────────────────────────────────────────────────
    public static InventoryItem? Get(string name)
        => _items.TryGetValue(name, out var item)
            ? CloneItem(item)   // clone ป้องกัน shared reference
            : null;

    public static bool IsKnown(string name) => _items.ContainsKey(name);

    // clone เพื่อให้แต่ละ instance ใน inventory เป็น object แยกกัน
    private static InventoryItem CloneItem(InventoryItem src) => new()
    {
        Name = src.Name,
        Icon = src.Icon,
        Description = src.Description,
        Type = src.Type,
        HpRestore = src.HpRestore,
        MpRestore = src.MpRestore,
        BonusAtk = src.BonusAtk,
        BonusDef = src.BonusDef,
        BonusMagic = src.BonusMagic,
        BonusMaxHp = src.BonusMaxHp,
        BonusMaxMp = src.BonusMaxMp,
    };
}