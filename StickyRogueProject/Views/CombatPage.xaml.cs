using StickyRogueProject.ViewModels;
using CommunityToolkit.Maui.Views;

namespace StickyRogueProject.Views;

public partial class CombatPage : ContentPage
{
    private readonly CombatViewModel _viewModel;
    private bool _isInitialized = false;

    public CombatPage(CombatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        // Wire Popup Delegates — ViewModel ไม่รู้จัก Page โดยตรง
        // Code-Behind เป็นตัวกลางเปิด Modal และส่ง Data ที่จำเป็น

        // Delegate เปิด InventoryPopUpPage
        _viewModel.OpenInventoryPopup = async () =>
        {
            var popup = new PopUp.InventoryPopUpPage(_viewModel.CurrentSave);
            await Navigation.PushModalAsync(popup);
        };

        // Delegate เปิด PlayerStatusPopUpPage
        _viewModel.OpenPlayerStatusPopup = async () =>
        {
            var popup = new PopUp.PlayerStatusPopUpPage(_viewModel.CurrentSave);
            await Navigation.PushModalAsync(popup);
        };

        // Delegate เปิด EnemyStatusPopUpPage
        _viewModel.OpenEnemyStatusPopup = async () =>
        {
            var popup = new PopUp.EnemyStatusPopUpPage(_viewModel.CurrentEnemy);
            await Navigation.PushModalAsync(popup);
        };

        // Delegate สำหรับ Alert (ให้ ViewModel ใช้ DisplayAlert โดยไม่ต้องรู้จัก Page)
        _viewModel.ShowAlert = async (title, message) =>
            await DisplayAlertAsync(title, message, "ตกลง");

        // Delegate สำหรับ Confirm Dialog
        _viewModel.ShowConfirm = async (title, message, accept, cancel) =>
            await DisplayAlertAsync(title, message, accept, cancel);
    }

    // เรียก InitializeCommand เฉพาะครั้งแรกที่หน้า CombatPage ปรากฏ
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