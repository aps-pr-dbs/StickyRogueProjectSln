using SQLite;
using StickyRogueProject.Models;

namespace StickyRogueProject.Data;

// DatabaseService
// ใช้ Pattern "Singleton" ผ่าน Dependency Injection (ลงทะเบียนใน MauiProgram.cs)
// ทุก Service อื่นๆ จะ Inject DatabaseService ตัวนี้ไปใช้
public class DatabaseService
{
    // ---- Connection หลักที่ใช้ติดต่อฐานข้อมูล ----
    // SQLiteAsyncConnection = เชื่อมต่อแบบ Async ไม่บล็อก UI Thread
    private readonly SQLiteAsyncConnection _db;


    // Constructor — รับ path ของไฟล์ฐานข้อมูลและสร้าง Connection
    // <param name="dbPath">ที่อยู่ไฟล์ .db3 บนอุปกรณ์ของผู้เล่น</param>
    public DatabaseService(string dbPath)
    {
        // สร้าง Connection ไปยังไฟล์ SQLite
        // ถ้าไฟล์ยังไม่มี SQLite จะสร้างให้อัตโนมัติ
        _db = new SQLiteAsyncConnection(dbPath);
    }

    // InitAsync — สร้างตารางทั้งหมดที่จำเป็นในฐานข้อมูล
    // ต้องเรียกใช้ครั้งเดียวตอนแอปเริ่มต้น (ใน MauiProgram.cs หรือ AppShell)
    // CreateTableAsync จะไม่ทำอะไรถ้าตารางมีอยู่แล้ว (Safe to call multiple times)
    public async Task InitAsync()
    {
        try
        {
            // สร้างตาราง ActiveSave (เก็บ Save ปัจจุบัน)
            await _db.CreateTableAsync<ActiveSave>();

            // สร้างตาราง RunHistory (เก็บประวัติการตายทุกครั้ง)
            await _db.CreateTableAsync<RunHistory>();
        }
        catch (Exception ex)
        {
            // ถ้าสร้างตารางไม่ได้ → แสดง Error ใน Console (ควร Log จริงใน Production)
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] InitAsync ล้มเหลว: {ex.Message}");
            throw; // โยน Exception ต่อให้ Caller จัดการ
        }
    }

    // คืนค่า Connection Object ให้ Service อื่นๆ ใช้งาน เช่น SaveService, HistoryService จะขอ Connection ผ่าน Method นี้
    public SQLiteAsyncConnection GetConnection() => _db;
}