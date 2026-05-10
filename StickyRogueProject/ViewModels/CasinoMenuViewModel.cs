using CommunityToolkit.Mvvm.Input;

namespace StickyRogueProject.ViewModels;

public partial class CasinoMenuViewModel
{
    // ⚡ คำสั่งวาร์ปไปหน้า Blackjack
    [RelayCommand]
    private async Task GoToBlackjackAsync()
    {
        // ใช้ PushModalAsync เพื่อเปิดหน้าเกมซ้อนขึ้นมา
        await Application.Current.MainPage.Navigation.PushModalAsync(new Views.BlackjackPage());
    }

    // ⚡ คำสั่งวาร์ปไปหน้า High Or Low
    [RelayCommand]
    private async Task GoToHighLowAsync()
    {
        await Application.Current.MainPage.Navigation.PushModalAsync(new Views.HighLowPage());
    }

    // ⚡ คำสั่งออกจากคาสิโน กลับไปหน้าต่อสู้
    [RelayCommand]
    private async Task ExitCasinoAsync()
    {
        // ใช้ // เพื่อรีเซ็ต Stack กลับไปหน้าหลัก
        await Shell.Current.GoToAsync("CombatPage");
    }
}