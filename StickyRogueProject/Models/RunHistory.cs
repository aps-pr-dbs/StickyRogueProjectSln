using SQLite;

namespace StickyRogueProject.Models;

[Table("RunHistory")]
public class RunHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string ClassName { get; set; } = string.Empty;

    [NotNull]
    public int LevelReached { get; set; }

    [NotNull]
    public int StageDiedAt { get; set; }

    // ⚡ เพิ่มตัวแปรสำหรับเก็บว่าตายที่ Loop ไหน
    [NotNull]
    public int LoopReached { get; set; } = 1;

    public int CoinsAtDeath { get; set; }

    public string CauseOfDeath { get; set; } = "ไม่ทราบสาเหตุ";

    [NotNull]
    public DateTime DiedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string SummaryText =>
        $"{ClassName}  •  Lv.{LevelReached}  •  Wave {StageDiedAt}  •  Loop {LoopReached}";

    [Ignore]
    public string DiedAtDisplay =>
        DiedAt.ToLocalTime().ToString("dd MMM yyyy  HH:mm");
}