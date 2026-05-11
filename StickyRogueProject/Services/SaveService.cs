using SQLite;
using StickyRogueProject.Data;
using StickyRogueProject.Models;

namespace StickyRogueProject.Services;

// SaveService 
// ทำหน้าที่ บันทึก, โหลด, และลบ Save ปัจจุบัน
// กฎ Permadeath ที่ต้องจำ:
// 1. โหลด Save : ถ้าไม่มีข้อมูล = ยังไม่เคยเล่น
// 2. ตัวละครตาย : ต้องเรียก DeleteSaveAsync() เสมอ
public class SaveService
{
    private readonly SQLiteAsyncConnection _db;

    public SaveService(DatabaseService databaseService)
    {
        // รับ Connection มาจาก DatabaseService ที่ Inject เข้ามา
        _db = databaseService.GetConnection();
    }

    // โหลด Save ปัจจุบัน — คืนค่า null ถ้าไม่มี Save อยู่
    // (ผู้เล่นยังไม่ได้เริ่มเกม หรือตายแล้ว)
    public async Task<ActiveSave?> LoadSaveAsync()
    {
        try
        {
            // ดึง Record แรกจากตาราง ActiveSave
            // FirstOrDefaultAsync คืน null ถ้าตารางว่างเปล่า
            return await _db.Table<ActiveSave>().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveService] โหลด Save ล้มเหลว: {ex.Message}");
            return null; // ถ้า Error → ถือว่าไม่มี Save
        }
    }

    // สร้าง Save ใหม่เมื่อกด "New Game"
    // <param name="save">Object ActiveSave ที่สร้างจาก ViewModel</param>
    public async Task CreateNewSaveAsync(ActiveSave save)
    {
        try
        {
            // ล้าง Save เก่าออกก่อน (กรณีมีข้อมูลค้างจากก่อนหน้า)
            await _db.DeleteAllAsync<ActiveSave>();

            // ตรวจสอบให้แน่ใจว่าปฏิบัติตามกฎเกมก่อน Insert
            save.Coins = 0;           // เหรียญเริ่มต้นที่ 0 เสมอ
            save.Inventory = new List<InventoryItem>(); // ล้างกระเป๋า
            save.Artifacts = new List<InventoryItem>(); // ล้าง Artifact

            // บันทึกลงฐานข้อมูล
            await _db.InsertAsync(save);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveService] สร้าง Save ใหม่ล้มเหลว: {ex.Message}");
            throw;
        }
    }

    // อัปเดต Save ที่มีอยู่ (เช่น หลังจบ Stage หรือเก็บไอเทม)
    public async Task UpdateSaveAsync(ActiveSave save)
    {
        try
        {
            await _db.UpdateAsync(save);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveService] อัปเดต Save ล้มเหลว: {ex.Message}");
            throw;
        }
    }

    // ลบ Save ปัจจุบันออก — เรียกใช้เมื่อตัวละครตาย (Permadeath!)
    // ต้องเรียก HistoryService.SaveRunHistoryAsync() ก่อนเรียก Method นี้
    public async Task DeleteSaveAsync()
    {
        try
        {
            // ลบทุก Record ในตาราง ActiveSave (ควรมีแค่ 1 Record)
            await _db.DeleteAllAsync<ActiveSave>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveService] ลบ Save ล้มเหลว: {ex.Message}");
            throw;
        }
    }

    // ตรวจสอบว่ามี Save อยู่หรือไม่ — ใช้ในหน้า Main Menu
    public async Task<bool> HasSaveAsync()
    {
        try
        {
            int count = await _db.Table<ActiveSave>().CountAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }
}