using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class MainPage : ContentPage
{
    // เก็บ Reference ของ ViewModel ไว้เรียกใช้ใน OnAppearing
    private readonly MainPageViewModel _viewModel;

    // Constructor รับ ViewModel เข้ามาผ่าน Dependency Injection
    // MauiProgram.cs ต้องลงทะเบียน MainPageViewModel ไว้ก่อน
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();

        // ผูก ViewModel กับ Page นี้
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    // OnAppearing — ทำงานทุกครั้งที่หน้านี้ปรากฏขึ้น
    // เรียก CheckSaveStatusCommand เพื่อเช็คว่ามี Save อยู่หรือไม่
    // สำคัญ: ต้องเรียกทุกครั้ง ไม่ใช่แค่ครั้งแรก
    // เพราะผู้เล่นอาจกลับมาหน้านี้หลังตาย (Save ถูกลบแล้ว)
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // เรียก Command ใน ViewModel → ไม่ใช่ Logic โดยตรง (ถูกกฎ MVVM)
        await _viewModel.CheckSaveStatusCommand.ExecuteAsync(null);
    }
}
