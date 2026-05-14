using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views; // ⚡ ต้องมีบรรทัดนี้เพื่อเรียกใช้ PopUp
using Microsoft.Maui.Controls;
using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;
using System;

namespace StickyRogueProject.Views;

public partial class MainPage : ContentPage
{
    private bool _isSoundMuted = false;
    private readonly MainPageViewModel _viewModel;

    private readonly SaveService _saveService;
    private readonly HistoryService _historyService;
    private readonly SoundService _soundService;

    public MainPage(MainPageViewModel viewModel, SaveService saveService, HistoryService historyService, SoundService soundService)
    {
        InitializeComponent();

        BindingContext = viewModel;
        _viewModel = viewModel;
        _saveService = saveService;
        _historyService = historyService;
        _soundService = soundService;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckSaveStatusCommand.ExecuteAsync(null);

        _soundService.PlayBgm();
    }

    // เปิดหน้า Load Game
    private async void OnLoadGameBtnClicked(object sender, EventArgs e)
    {
        _soundService.PlayClickSound();
        await Navigation.PushModalAsync(new PopUp.LoadGamePopUp(_saveService));
    }

    // เปิดหน้า History
    private async void OnHistoryBtnClicked(object sender, EventArgs e)
    {
        _soundService.PlayClickSound();
        await Navigation.PushModalAsync(new PopUp.HistoryPopUp(_historyService));
    }

    private void OnStartGameBtnClicked(object sender, EventArgs e)
    {
        _soundService.PlayClickSound();
    }

    // ⚡ ฟังก์ชันสำหรับปุ่มตั้งค่า ⚡
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        _soundService.PlayClickSound();
        await this.ShowPopupAsync(new PopUp.SettingsPopUpPage(_soundService));

    }
}