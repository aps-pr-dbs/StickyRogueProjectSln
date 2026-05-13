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
            // 1. สร้างและบันทึกประวัติการตายใหม่ล่าสุดลงไปก่อน
            var history = new RunHistory
            {
                ClassName = completedRun.ClassName,
                LevelReached = completedRun.Level,
                StageDiedAt = completedRun.CurrentWave,
                LoopReached = completedRun.CurrentLoop,
                CoinsAtDeath = completedRun.Coins,
                CauseOfDeath = causeOfDeath,
                DiedAt = DateTime.UtcNow
            };

            await _db.InsertAsync(history);

            // ⚡ 2. เริ่มระบบ FIFO (จำกัดสูงสุด 5 รายการ) ⚡
            // ดึงข้อมูลทั้งหมดในตารางมาเรียงลำดับจาก "ใหม่ที่สุด -> เก่าที่สุด"
            var allHistory = await _db.Table<RunHistory>()
                                      .OrderByDescending(h => h.DiedAt)
                                      .ToListAsync();

            // เช็คว่าถ้ามีประวัติเกิน 5 รายการ
            if (allHistory.Count > 5)
            {
                // ใช้คำสั่ง Skip(5) เพื่อข้าม 5 อันดับแรก (ที่ใหม่สุด) ไป 
                // แล้วเอาข้อมูลอันที่ 6 เป็นต้นไป (ที่เก่ากว่า) มาจับยัดลง List เพื่อเตรียมลบ
                var recordsToDelete = allHistory.Skip(5).ToList();

                // สั่งลบประวัติที่เก่าเกินโควต้าทิ้งทีละอัน
                foreach (var record in recordsToDelete)
                {
                    await _db.DeleteAsync(record);
                }

                System.Diagnostics.Debug.WriteLine($"[HistoryService] ทำงานแบบ FIFO: ลบประวัติเก่าทิ้ง {recordsToDelete.Count} รายการ");
            }
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