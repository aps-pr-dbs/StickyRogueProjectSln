using StickyRogueProject.Models;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views.PopUp;

public partial class InGameCharacterStatus : ContentPage
{
    private readonly InGameCharacterStatusViewModel _viewModel;

    // ⚡ เพิ่มตัวแปรล็อกการกดเบิ้ล
    private bool _isClosing = false;

    public InGameCharacterStatus(ActiveSave currentSave, int currentXp, int xpToNextLevel)
    {
        InitializeComponent();

        _viewModel = new InGameCharacterStatusViewModel(currentSave, currentXp, xpToNextLevel, async () => await ClosePopup());
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(50);

        _ = HpBar.ProgressTo(_viewModel.HpProgress, 250, Easing.Linear);
        _ = MpBar.ProgressTo(_viewModel.MpProgress, 250, Easing.Linear);
        _ = XpBar.ProgressTo(_viewModel.XpProgress, 250, Easing.Linear);
    }

    // ⚡ แก้ไขฟังก์ชันนี้ให้ป้องกันการกดรัวๆ
    private async Task ClosePopup()
    {
        if (_isClosing) return; // ถ้ากำลังปิดอยู่ ให้เมินการกดครั้งต่อไปเลย
        _isClosing = true;

        await Navigation.PopModalAsync();
    }
}