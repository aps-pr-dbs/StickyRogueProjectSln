using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

// MainPageViewModel คือ ViewModel ของหน้า Main Menu
// สืบทอดมาจาก ObservableObject ของ CommunityToolkit.Mvvm
// ObservableObject จัดการ INotifyPropertyChanged ให้อัตโนมัติ ไม่ต้องเขียนเอง
public partial class MainPageViewModel : ObservableObject
{
    // --- Dependency: SaveService ---
    // Inject SaveService เข้ามาเพื่อเช็คว่ามี Save อยู่หรือไม่
    // ใช้กำหนดว่าปุ่ม "Load Game" จะเปิดใช้ได้หรือไม่
    private readonly SaveService _saveService;

    // --- State: HasExistingSave ---
    // [ObservableProperty] ให้ CommunityToolkit สร้าง Property "HasExistingSave" ให้อัตโนมัติ
    // XAML จะ Bind กับ Property นี้เพื่อเปิด/ปิดปุ่ม Load Game
    [ObservableProperty]
    private bool _hasExistingSave;

    // --- State: IsLoading ---
    // ใช้แสดง ActivityIndicator ขณะกำลังเช็ค Database ตอน Page โหลด
    [ObservableProperty]
    private bool _isLoading;

    // --- Constructor ---
    // รับ SaveService เข้ามาผ่าน Dependency Injection (ลงทะเบียนใน MauiProgram.cs)
    public MainPageViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    // --- Command: CheckSaveStatusCommand ---
    // เรียกใช้เมื่อหน้า Main Menu ปรากฏขึ้น (เรียกจาก OnAppearing ใน Code-Behind)
    // ตรวจสอบว่ามี Save อยู่ใน Database หรือไม่ แล้วอัปเดต HasExistingSave
    [RelayCommand]
    private async Task CheckSaveStatusAsync()
    {
        try
        {
            // แสดง Loading ระหว่างเช็ค Database
            IsLoading = true;

            // ถาม SaveService ว่ามี Save อยู่หรือเปล่า
            HasExistingSave = await _saveService.HasSaveAsync();
        }
        catch (Exception ex)
        {
            // ถ้า Database มีปัญหา ให้ถือว่าไม่มี Save (ปลอดภัยกว่า)
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] เช็ค Save ล้มเหลว: {ex.Message}");
            HasExistingSave = false;
        }
        finally
        {
            // ซ่อน Loading ไม่ว่าจะสำเร็จหรือ Error
            IsLoading = false;
        }
    }

    // --- Command: NewGameCommand ---
    // เรียกเมื่อผู้เล่นกดปุ่ม "New Game"
    // นำทางไปยัง ClassSelectPage เพื่อให้เลือก Class ตัวละคร
    [RelayCommand]
    private async Task NewGameAsync()
    {
        try
        {
            // ใช้ Shell Navigation ไปหน้าเลือก Class
            // Route "ClassSelectPage" ต้องลงทะเบียนใน AppShell.xaml.cs ก่อน
            await Shell.Current.GoToAsync("StoryPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Navigate New Game ล้มเหลว: {ex.Message}");

            // แสดง Alert ถ้า Navigation ไม่ได้
            await Shell.Current.DisplayAlert(
                "เกิดข้อผิดพลาด",
                "ไม่สามารถเริ่มเกมใหม่ได้ กรุณาลองอีกครั้ง",
                "ตกลง");
        }
    }

    // --- Command: LoadGameCommand ---
    // เรียกเมื่อผู้เล่นกดปุ่ม "Load Game"
    // CanExecute ผูกกับ HasExistingSave → ปุ่มจะ Disable อัตโนมัติถ้าไม่มี Save
    [RelayCommand(CanExecute = nameof(CanLoadGame))]
    private async Task LoadGameAsync()
    {
        try
        {
            // นำทางไปหน้าเกมหลัก พร้อมส่ง Parameter บอกว่าเป็นการ "Load"
            await Shell.Current.GoToAsync("GamePage?mode=load");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Navigate Load Game ล้มเหลว: {ex.Message}");

            await Shell.Current.DisplayAlert(
                "เกิดข้อผิดพลาด",
                "ไม่สามารถโหลดเกมได้ กรุณาลองอีกครั้ง",
                "ตกลง");
        }
    }

    // เงื่อนไขสำหรับ LoadGameCommand
    // คืนค่า true เมื่อมี Save อยู่ → ปุ่ม Load Game จะ Enable
    // คืนค่า false เมื่อไม่มี Save → ปุ่มจะ Disable อัตโนมัติ
    private bool CanLoadGame() => HasExistingSave;

    // --- Command: HistoryCommand ---
    // เรียกเมื่อผู้เล่นกดปุ่ม "History"
    // นำทางไปยังหน้าแสดงประวัติการตายทั้งหมด
    [RelayCommand]
    private async Task GoToHistoryAsync()
    {
        try
        {
            // ไปหน้า HistoryPage ผ่าน Shell Navigation
            await Shell.Current.GoToAsync("HistoryPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Navigate History ล้มเหลว: {ex.Message}");

            await Shell.Current.DisplayAlert(
                "เกิดข้อผิดพลาด",
                "ไม่สามารถเปิดหน้าประวัติได้",
                "ตกลง");
        }
    }

    // เมื่อ HasExistingSave เปลี่ยนค่า → แจ้ง LoadGameCommand ให้ประเมิน CanExecute ใหม่
    // CommunityToolkit จะสร้าง Method OnHasExistingSaveChanged ให้อัตโนมัติผ่าน partial
    partial void OnHasExistingSaveChanged(bool value)
    {
        // แจ้งให้ LoadGameCommand รู้ว่าเงื่อนไข CanExecute อาจเปลี่ยนแล้ว
        LoadGameCommand.NotifyCanExecuteChanged();
    }
}
