using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class CasinoMenu : ContentPage
{
    public CasinoMenu()
    {
        InitializeComponent();
    }

    private async void BtnBlackjack_Clicked(object sender, EventArgs e)
    {
        // ดึงระบบ SaveService ออกมาเพื่อเตรียมส่งให้หน้า Blackjack
        var saveService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<SaveService>();

        // สร้าง ViewModel พร้อมส่ง SaveService ให้มัน
        var viewModel = new BlackjackViewModel(saveService);

        // เปิดหน้า Blackjack พร้อมแนบ ViewModel ไปด้วย (แก้บัค CS7036)
        await Navigation.PushModalAsync(new BlackjackPage(viewModel));
    }

    private async void BtnHighLow_Clicked(object sender, EventArgs e)
    {
        // ดึงระบบ SaveService ออกมาเพื่อเตรียมส่งให้หน้า High Or Low
        var saveService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<SaveService>();

        // สร้าง ViewModel พร้อมส่ง SaveService ให้มัน
        var viewModel = new HighLowViewModel(saveService);

        // เปิดหน้า High Or Low พร้อมแนบ ViewModel ไปด้วย
        await Navigation.PushModalAsync(new HighLowPage(viewModel));
    }

    private async void BtnExit_Clicked(object sender, EventArgs e)
    {
        // สั่งวาร์ปกลับไปลุยหน้าต่อสู้ต่อ
        await Shell.Current.GoToAsync("CombatPage");
    }
}