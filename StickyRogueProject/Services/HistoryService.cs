using SQLite;
using StickyRogueProject.Data;
using StickyRogueProject.Models;

namespace StickyRogueProject.Services;

/// <summary>
/// HistoryService — รับผิดชอบการจัดการตาราง RunHistory
/// ใช้บันทึกสถิติทุกครั้งที่ผู้เล่นตาย
/// และโหลดข้อมูลมาแสดงในหน้า History
/// </summary>
public class HistoryService
{
    private readonly SQLiteAsyncConnection _db;

    public HistoryService(DatabaseService databaseService)
    {
        _db = databaseService.GetConnection();
    }

    /// <summary>
    /// บันทึกประวัติการตายลงฐานข้อมูล
    /// ต้องเรียกก่อน SaveService.DeleteSaveAsync() เสมอ!
    /// 
    /// ลำดับที่ถูกต้องเมื่อผู้เล่นตาย:
    ///   1. เรียก SaveRunHistoryAsync(activeSave)  ← บันทึกสถิติก่อน
    ///   2. เรียก SaveService.DeleteSaveAsync()    ← ลบ Save หลัง
    /// </summary>
    public async Task SaveRunHistoryAsync(ActiveSave completedRun, string causeOfDeath = "ไม่ทราบสาเหตุ")
    {
        try
        {
            // แปลงข้อมูลจาก ActiveSave → RunHistory
            var history = new RunHistory
            {
                ClassName = completedRun.ClassName,
                LevelReached = completedRun.Level,
                StageDiedAt = completedRun.CurrentWave, // เปลี่ยนจาก CurrentStage เป็น CurrentWave
                CoinsAtDeath = completedRun.Coins,
                CauseOfDeath = causeOfDeath,
                DiedAt = DateTime.UtcNow
            };

            await _db.InsertAsync(history);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] บันทึกประวัติล้มเหลว: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// โหลดประวัติการตายทั้งหมด เรียงจากใหม่ → เก่า
    /// ใช้แสดงในหน้า History Page
    /// </summary>
    public async Task<List<RunHistory>> GetAllHistoryAsync()
    {
        try
        {
            // OrderByDescending = เรียงจากใหม่ที่สุดก่อน
            return await _db.Table<RunHistory>()
                            .OrderByDescending(h => h.DiedAt)
                            .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] โหลดประวัติล้มเหลว: {ex.Message}");
            return new List<RunHistory>(); // คืน List ว่างเปล่าถ้า Error
        }
    }

    /// <summary>
    /// ล้างประวัติทั้งหมด (ถ้าผู้เล่นต้องการ Reset)
    /// </summary>
    public async Task ClearAllHistoryAsync()
    {
        try
        {
            await _db.DeleteAllAsync<RunHistory>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] ล้างประวัติล้มเหลว: {ex.Message}");
            throw;
        }
    }
}