using CommunityToolkit.Maui.Views;
using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class CombatPage : ContentPage
{
    private readonly CombatViewModel _viewModel;
    private readonly SoundService _soundService;
    private readonly SaveService _saveService; // ⚡ ต้องเพิ่มตัวนี้ด้วยเพราะมีการใช้ใน selectClassBtnClicked
    private bool _isInitialized = false;

    public CombatPage(CombatViewModel viewModel, SoundService soundService, SaveService saveService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _soundService = soundService;
        _saveService = saveService;

        _viewModel.OpenInventoryPopup = async () =>
        {
            var popup = new PopUp.InventoryPopUpPage(_viewModel.CurrentSave);
            await Navigation.PushModalAsync(popup);
        };

        // ⚡ แก้ไขตรงนี้แล้ว! ⚡
        // ⚡ แก้ไขตรงนี้แล้ว! ⚡
        _viewModel.OpenPlayerStatusPopup = async () =>
        {
            // ส่งค่าไปให้ครบ 3 ตัว ตามที่ไฟล์ InGameCharacterStatus ต้องการ
            var popup = new PopUp.InGameCharacterStatus(
                _viewModel.CurrentSave,
                _viewModel.CurrentXp,
                _viewModel.XpToNextLevel
            );
            await Navigation.PushModalAsync(popup);
        };

        _viewModel.OpenEnemyStatusPopup = async () =>
        {
            var popup = new PopUp.EnemyStatusPopUpPage(_viewModel.CurrentEnemy);
            await Navigation.PushModalAsync(popup);
        };

        _viewModel.ShowAlert = async (title, message) =>
            await DisplayAlertAsync(title, message, "ตกลง");

        _viewModel.ShowConfirm = async (title, message, accept, cancel) =>
            await DisplayAlertAsync(title, message, accept, cancel);
    }

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