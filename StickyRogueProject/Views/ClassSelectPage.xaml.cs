using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class ClassSelectPage : ContentPage
{
    private readonly ClassSelectViewModel _viewModel;
    private readonly SoundService _soundService;
    private readonly SaveService _saveService; // ⚡ ต้องเพิ่มตัวนี้ด้วยเพราะมีการใช้ใน selectClassBtnClicked

    // Constructor รับ ViewModel และ Services ต่างๆ จาก DI Container
    public ClassSelectPage(ClassSelectViewModel viewModel, SoundService soundService, SaveService saveService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _soundService = soundService;
        _saveService = saveService;

        // ผูก ViewModel กับ Page
        BindingContext = _viewModel;
    }

    // ฟังก์ชันเมื่อกดเลือก Class
    private async void selectClassRightBtnClicked(object sender, EventArgs e)
    {
        // ⚡ 1. เล่นเสียงคลิก
        _soundService.PlayClickSound();
    }

    private async void selectClassleftBtnClicked(object sender, EventArgs e)
    {
        // ⚡ 1. เล่นเสียงคลิก
        _soundService.PlayClickSound();

        // ⚡ 2. เปิด Popup 
    }

    private async void selectClassBtnClicked(object sender, EventArgs e)
    {
        // ⚡ 1. เล่นเสียงคลิก
        _soundService.PlaySelectSound();

    }


    // OnBackClicked — จัดการการกดปุ่ม Back
    private async void OnBackClicked(object sender, EventArgs e)
    {
        // ⚡ 1. เล่นเสียงคลิกก่อนย้อนกลับ
        _soundService.PlayClickSound();

        // ⚡ 2. ย้อนกลับไปหน้าหลัก
        await Shell.Current.GoToAsync("MainPage");
    }
    //ป้องกันการกดปุ่ม Back (ย้อนกลับ) ของ Android / Windows
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}