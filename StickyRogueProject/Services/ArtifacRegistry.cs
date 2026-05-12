using StickyRogueProject.Models;

namespace StickyRogueProject.Services;

public static class ArtifactRegistry
{
    // Dictionary เก็บข้อมูลต้นฉบับ (Master Data) ของ Artifact ทั้งหมด
    private static readonly Dictionary<string, ArtifactItem> _masterArtifacts = new()
    {
        // --- หมวด ATK ---
        { "art_atk_1", new ArtifactItem { Key="art_atk_1", Name="Catfood Hammer",    Description="+2 ATK ต่อ Lv",       StatBonus=2,  StatType="ATK",   Price=25, ImageSource="catfood_hammer.png"      } },
        { "art_atk_2", new ArtifactItem { Key="art_atk_2", Name="Fishbone Sword",    Description="+3 ATK ต่อ Lv",       StatBonus=3,  StatType="ATK",   Price=40, ImageSource="fishbone_sword.png"      } },
        { "art_atk_3", new ArtifactItem { Key="art_atk_3", Name="Catlitter Blaster", Description="+5 ATK ต่อ Lv",       StatBonus=5,  StatType="ATK",   Price=70, ImageSource="catlitter_blaster.png"   } },
        
        // --- หมวด DEF ---
        { "art_def_1", new ArtifactItem { Key="art_def_1", Name="Cardbox Armor",     Description="+2 DEF ต่อ Lv",       StatBonus=2,  StatType="DEF",   Price=25, ImageSource="cardbox_armor.png"       } },
        { "art_def_2", new ArtifactItem { Key="art_def_2", Name="Laundry Helmet",    Description="+3 DEF ต่อ Lv",       StatBonus=3,  StatType="DEF",   Price=40, ImageSource="laundrybasket_helmet.png" } },
        { "art_def_3", new ArtifactItem { Key="art_def_3", Name="Litterbox Armor",   Description="+5 DEF ต่อ Lv",       StatBonus=5,  StatType="DEF",   Price=70, ImageSource="litterbox_armor.png"     } },
        
        // --- หมวด INT ---
        { "art_int_1", new ArtifactItem { Key="art_int_1", Name="Noodle Compass",    Description="+2 INT ต่อ Lv",       StatBonus=2,  StatType="INT",   Price=25, ImageSource="noodle_compass.png"      } },
        { "art_int_2", new ArtifactItem { Key="art_int_2", Name="Goldfish Staff",    Description="+3 INT ต่อ Lv",       StatBonus=3,  StatType="INT",   Price=40, ImageSource="goldfish_staff.png"      } },
        { "art_int_3", new ArtifactItem { Key="art_int_3", Name="Human Tamer Tome",  Description="+5 INT ต่อ Lv",       StatBonus=5,  StatType="INT",   Price=70, ImageSource="humantamer_tome.png"     } },
        
        // --- หมวด HP ---
        { "art_hp_1",  new ArtifactItem { Key="art_hp_1",  Name="Catfood Backpack",  Description="+10 HP สูงสุด ต่อ Lv", StatBonus=10, StatType="HP",  Price=30, ImageSource="catfood_backpack.png"    } },
        { "art_hp_2",  new ArtifactItem { Key="art_hp_2",  Name="King Meow Collar",  Description="+20 HP สูงสุด ต่อ Lv", StatBonus=20, StatType="HP",  Price=55, ImageSource="kingmeow_collar.png"    } },
        { "art_hp_3",  new ArtifactItem { Key="art_hp_3",  Name="9 Lives Collar",    Description="+30 HP สูงสุด ต่อ Lv", StatBonus=30, StatType="HP",  Price=80, ImageSource="ninelives_collar.png"   } },
        
        // --- หมวด MP (MAXMP) ---
        { "art_mp_1",  new ArtifactItem { Key="art_mp_1",  Name="Catwitch Hat",      Description="+5 Max MP ต่อ Lv",    StatBonus=5,  StatType="MAXMP", Price=25, ImageSource="catwitch_hat.png"        } },
        { "art_mp_2",  new ArtifactItem { Key="art_mp_2",  Name="Goldfish Orb",      Description="+10 Max MP ต่อ Lv",   StatBonus=10, StatType="MAXMP", Price=40, ImageSource="goldfish_orb.png"        } },
        { "art_mp_3",  new ArtifactItem { Key="art_mp_3",  Name="Ancient Meow Tome", Description="+20 Max MP ต่อ Lv",   StatBonus=20, StatType="MAXMP", Price=70, ImageSource="ancietmeow_tome.png"     } },
    };

    // 1. ดึงไอเทม 1 ชิ้นด้วย Key (ทำการ Clone เพื่อไม่ให้กระทบต้นฉบับ)
    public static ArtifactItem? GetArtifact(string key)
    {
        if (_masterArtifacts.TryGetValue(key, out var item))
        {
            return new ArtifactItem
            {
                Key = item.Key,
                Name = item.Name,
                Description = item.Description,
                StatBonus = item.StatBonus,
                StatType = item.StatType,
                Price = item.Price,
                ImageSource = item.ImageSource,
                // Lv เริ่มต้นที่ 0 (หรือตามที่คุณอ๊าฟเซ็ตไว้ใน Model)
            };
        }
        return null;
    }

    // 2. ดึงลิสต์ไอเทมทั้งหมด (เอาไปใช้สร้างของขายในหน้า ShopViewModel)
    public static List<ArtifactItem> GetAllArtifacts()
    {
        return _masterArtifacts.Values.Select(item => GetArtifact(item.Key)!).ToList();
    }

    // 3. สุ่มไอเทม 1 ชิ้น (เอาไปใช้ดรอปตอนสู้ชนะใน CombatViewModel)
    public static ArtifactItem GetRandomArtifact()
    {
        var keys = _masterArtifacts.Keys.ToList();
        var randomKey = keys[new Random().Next(keys.Count)];
        return GetArtifact(randomKey)!;
    }
}