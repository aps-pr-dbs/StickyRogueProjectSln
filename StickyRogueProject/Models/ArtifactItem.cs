using CommunityToolkit.Mvvm.ComponentModel;

namespace StickyRogueProject.Models;

// partial class คำสำคัญที่ CommunityToolkit ต้องการเพื่อ Generate Code
public partial class ArtifactItem : ObservableObject
{
    // ชื่อ Artifact ที่แสดงในร้าน เช่น "Sword of Power"
    [ObservableProperty]
    private string _name = string.Empty;

    // คำอธิบายว่า Artifact นี้ Boost Stat อะไร เช่น "+2 ATK ต่อ Level"
    [ObservableProperty]
    private string _description = string.Empty;

    // Emoji ไอคอนประจำ Artifact — ใช้แทนรูปภาพ
    [ObservableProperty]
    private string _icon = string.Empty;

    // ชื่อไฟล์รูปภาพสำหรับนำไปแสดงผลบน UI
    public string ImageSource { get; set; } = string.Empty;

    // ราคาซื้อ Artifact (หน่วย: เหรียญ)
    [ObservableProperty]
    private int _price;

    // ค่า Stat ที่เพิ่มขึ้นต่อ 1 Level — เช่น 2 หมายถึง +2 ต่อ Level
    [ObservableProperty]
    private int _statBonus;

    // ชนิด Stat ที่ Artifact นี้เพิ่ม — ใช้ใน ShopViewModel ตอนคิดคำนวณ
    // ค่าที่รับได้: "ATK", "DEF", "INT", "SPD", "HP"
    [ObservableProperty]
    private string _statType = string.Empty;

    // Level ปัจจุบันของ Artifact นี้ในการเล่นครั้งนี้
    // [NotifyPropertyChangedFor] บอก CommunityToolkit ว่าเมื่อ CurrentLevel เปลี่ยน
    // ให้แจ้ง LevelDisplay และ IsMaxLevel ให้อัปเดต UI ด้วย
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelDisplay))]
    [NotifyPropertyChangedFor(nameof(IsMaxLevel))]
    [NotifyPropertyChangedFor(nameof(BuyButtonText))]
    [NotifyPropertyChangedFor(nameof(BuyButtonOpacity))]
    private int _currentLevel = 0;

    // Level สูงสุดของ Artifact ทุกตัว — กำหนดไว้ที่ 10 ตามกฎเกม
    public int MaxLevel => 10;

    // ===== Computed Properties =====
    // Properties เหล่านี้คำนวณมาจาก CurrentLevel และ MaxLevel
    // ไม่ต้องมี Backing Field เพราะ XAML จะดึงค่าเมื่อ CurrentLevel เปลี่ยน

    // ข้อความแสดง Level เช่น "Lv. 3 / 10"
    // XAML จะ Bind ตรงๆ ไม่ต้องแปลงใน Converter
    public string LevelDisplay => $"Lv. {CurrentLevel} / {MaxLevel}";

    // ตรวจสอบว่า Artifact ถึง Max Level หรือยัง
    // ใช้ใน XAML เพื่อ Disable ปุ่ม Buy และเปลี่ยนข้อความเป็น "MAX"
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;

    // ข้อความบนปุ่ม Buy — เปลี่ยนเป็น "MAX" เมื่อถึง Level 10
    public string BuyButtonText => IsMaxLevel ? "MAX" : $"ซื้อ  {Price} 🪙";

    // Opacity ของปุ่ม Buy — ลดความสว่างเมื่อ Disable (IsMaxLevel = true)
    public double BuyButtonOpacity => IsMaxLevel ? 0.35 : 1.0;

    // Key ที่ใช้ในการ Serialize/Deserialize JSON — ต้อง Unique ทุกตัว
    // เช่น "sword_of_power", "iron_armor"
    public string Key { get; set; } = string.Empty;
}
