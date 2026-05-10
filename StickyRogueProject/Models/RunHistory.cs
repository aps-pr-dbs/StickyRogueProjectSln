using SQLite;

namespace StickyRogueProject.Models;

/// <summary>
/// Model สำหรับเก็บ "ประวัติการตาย" ของผู้เล่น
/// ทุกครั้งที่ตัวละครตาย → บันทึก Record ใหม่ลงตารางนี้ก่อน
/// จากนั้นจึงค่อยลบ ActiveSave ออก (Permadeath System)
/// ข้อมูลนี้จะแสดงในหน้า History ของเกม
/// </summary>
[Table("RunHistory")]
public class RunHistory
{
    /// <summary>
    /// Primary Key — SQLite สร้างให้อัตโนมัติ
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // ---- สถิติของการเล่นครั้งนั้น ----

    /// <summary>
    /// Class ที่ผู้เล่นเลือกในการเล่นครั้งนี้ เช่น "Warrior"
    /// </summary>
    [NotNull]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Level สูงสุดที่ทำได้ก่อนจะตาย
    /// </summary>
    [NotNull]
    public int LevelReached { get; set; }

    /// <summary>
    /// Stage ที่ตาย — บอกว่าไปถึงแค่ไหน
    /// </summary>
    [NotNull]
    public int StageDiedAt { get; set; }

    /// <summary>
    /// จำนวนเหรียญสะสมสูงสุดก่อนตาย (ไว้ Flex)
    /// </summary>
    public int CoinsAtDeath { get; set; }

    /// <summary>
    /// สาเหตุการตาย เช่น "ถูก Slime ฆ่า", "ตกดาน" ฯลฯ
    /// เอาไว้แสดงให้ผู้เล่นน้ำตาไหล
    /// </summary>
    public string CauseOfDeath { get; set; } = "ไม่ทราบสาเหตุ";

    /// <summary>
    /// วันเวลาที่ตาย — บันทึกเป็น UTC
    /// </summary>
    [NotNull]
    public DateTime DiedAt { get; set; } = DateTime.UtcNow;

    // ---- Computed Display Properties ----

    /// <summary>
    /// ข้อความสรุปสำหรับแสดงในหน้า History
    /// [Ignore] = SQLite จะไม่สร้าง Column นี้
    /// </summary>
    [Ignore]
    public string SummaryText =>
        $"{ClassName}  •  Lv.{LevelReached}  •  Stage {StageDiedAt}";

    /// <summary>
    /// แสดงวันที่ตายในรูปแบบที่อ่านง่าย (Local Time)
    /// </summary>
    [Ignore]
    public string DiedAtDisplay =>
        DiedAt.ToLocalTime().ToString("dd MMM yyyy  HH:mm");
}