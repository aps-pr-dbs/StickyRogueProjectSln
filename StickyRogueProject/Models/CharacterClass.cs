
// CharacterClass คือ Model สำหรับเก็บข้อมูลของแต่ละ Class ตัวละคร
// ไม่ได้ใช้กับ SQLite โดยตรง → ไม่ต้องมี [Table] Annotation
// ใช้เป็นข้อมูล "ต้นทาง" ก่อนที่จะสร้าง ActiveSave ลง Database

namespace StickyRogueProject.Models;

public class CharacterClass
{
    // ชื่อ Class เช่น "Warrior", "Rogue", "Mage"
    public string Name { get; set; } = string.Empty;

    // คำอธิบายสั้นๆ ของ Class — แสดงใต้ชื่อในหน้า ClassSelect
    public string Description { get; set; } = string.Empty;

    // ชื่อไฟล์ภาพ Placeholder ของตัวละคร เช่น "warrior.png"
    // ต้องวางไฟล์ภาพไว้ใน Resources/Images/ ของโปรเจกต์
    public string ImageSource { get; set; } = string.Empty;

    // ค่าพลังโจมตี (Attack) พื้นฐาน
    public int BaseAtk { get; set; }

    // ค่าพลังป้องกัน (Defense) พื้นฐาน
    public int BaseDef { get; set; }

    // ค่าปัญญา (Intelligence) พื้นฐาน — ใช้กับสาย Magic
    public int BaseInt { get; set; }

    // ค่าความเร็ว (MaxMp) พื้นฐาน 
    public int BaseMaxMp { get; set; }

    // HP สูงสุดเริ่มต้น — จะถูกคัดลอกไปยัง ActiveSave.MaxHp และ ActiveSave.CurrentHp
    public int BaseMaxHp { get; set; }

    // Emoji สีสำหรับตกแต่ง UI — แสดงข้างชื่อ Class
    public string ThemeEmoji { get; set; } = "⚔️";

    // สี Accent ของแต่ละ Class (Hex String) — ใช้กับ Border และ Label ใน XAML
    public string AccentColor { get; set; } = "#7B4FBF";
}
