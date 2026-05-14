using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class ShopPage : ContentPage
{
    private readonly ShopViewModel _viewModel;

    public ShopPage(ShopViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }

    // ===== เพิ่มฟังก์ชันนี้เข้าไปครับ =====
    private async void OnExitClicked(object sender, EventArgs e)
    {
        // คำสั่ง ".." หมายถึงการถอยกลับไป 1 หน้า (กลับไป Main Menu)
        await Shell.Current.GoToAsync("..");
    }
    //ป้องกันการกดปุ่ม Back (ย้อนกลับ) ของ Android / Windows
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}