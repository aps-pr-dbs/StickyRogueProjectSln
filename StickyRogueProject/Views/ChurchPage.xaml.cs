// Code-Behind ขั้นต่ำ — ไม่มี Business Logic
// หน้าที่: รับ ViewModel ผ่าน DI และเรียก InitializeCommand

using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class ChurchPage : ContentPage
{
    private readonly ChurchViewModel _viewModel;

    public ChurchPage(ChurchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    // เรียก Init ทุกครั้งที่ปรากฏ (รวมถึงเมื่อกลับจาก SaveAndExit ถูก Cancel)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
