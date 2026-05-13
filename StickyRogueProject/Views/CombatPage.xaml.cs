using CommunityToolkit.Maui.Views;
using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class CombatPage : ContentPage
{
    private readonly CombatViewModel _viewModel;
    private readonly SoundService _soundService;
    private readonly SaveService _saveService;
    private bool _isInitialized = false;

    public CombatPage(CombatViewModel viewModel, SoundService soundService, SaveService saveService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _soundService = soundService;
        _saveService = saveService;

        // ── ระบบ Popup ──
        _viewModel.OpenInventoryPopup = async () =>
        {
            await Navigation.PushModalAsync(new PopUp.InventoryPopUpPage(_viewModel.CurrentSave));
        };

        _viewModel.OpenPlayerStatusPopup = async () =>
        {
            await Navigation.PushModalAsync(new PopUp.InGameCharacterStatus(
                _viewModel.CurrentSave, _viewModel.CurrentXp, _viewModel.XpToNextLevel));
        };

        _viewModel.OpenEnemyStatusPopup = async () =>
        {
            await Navigation.PushModalAsync(new PopUp.EnemyStatusPopUpPage(_viewModel.CurrentEnemy));
        };

        _viewModel.ShowAlert = async (title, message) => await DisplayAlert(title, message, "ตกลง");

        // ── ⚡ ระบบอนิเมชัน (Game Feel) ⚡ ──

        // 1. ตัวละครโดนตี (เปลี่ยนเป็นสั่นรัวๆ)
        // 1. ตัวละครโดนตี (เหลือแค่สั่นรูปภาพ ไม่กระพริบแดงแล้ว)
        _viewModel.OnPlayerHitAnim = async () => {
            uint speed = 40;
            await PlayerImage.TranslateTo(-15, 0, speed);
            await PlayerImage.TranslateTo(15, 0, speed);
            await PlayerImage.TranslateTo(-10, 0, speed);
            await PlayerImage.TranslateTo(10, 0, speed);
            await PlayerImage.TranslateTo(0, 0, speed);
        };

        // 2. มอนสเตอร์โดนตี (เหลือแค่สั่นรูปภาพ ไม่กระพริบแดงแล้ว)
        _viewModel.OnEnemyHitAnim = async () => {
            uint speed = 40;
            await EnemyImage.TranslateTo(-15, 0, speed);
            await EnemyImage.TranslateTo(15, 0, speed);
            await EnemyImage.TranslateTo(-10, 0, speed);
            await EnemyImage.TranslateTo(10, 0, speed);
            await EnemyImage.TranslateTo(0, 0, speed);
        };

        // 3. ตัวละครหลบได้ (โยกซ้าย)
        _viewModel.OnPlayerDodgeAnim = async () => {
            await PlayerImage.TranslateTo(-30, 0, 90, Easing.SinOut);
            await PlayerImage.TranslateTo(0, 0, 90, Easing.SinIn);
        };

        // 4. มอนสเตอร์หลบได้ (โยกขวา)
        _viewModel.OnEnemyDodgeAnim = async () => {
            await EnemyImage.TranslateTo(30, 0, 90, Easing.SinOut);
            await EnemyImage.TranslateTo(0, 0, 90, Easing.SinIn);
        };
    }

    private void HPPotionBtnClicked(object sender, EventArgs e) => _soundService.PlayHPPotionSound();
    private void ManaPotionBtnClicked(object sender, EventArgs e) => _soundService.PlayManaSound();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_isInitialized)
        {
            await _viewModel.InitializeCommand.ExecuteAsync(null);
            _isInitialized = true;
        }
    }
}