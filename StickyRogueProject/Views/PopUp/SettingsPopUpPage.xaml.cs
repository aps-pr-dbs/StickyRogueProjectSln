using CommunityToolkit.Maui.Views;
using StickyRogueProject.Services;
using Microsoft.Maui.Controls;
using System;

namespace StickyRogueProject.Views.PopUp;

// ⚡ เปลี่ยนจาก ContentPage เป็น Popup
public partial class SettingsPopUpPage : Popup
{
    private readonly SoundService _soundService;

    public SettingsPopUpPage(SoundService soundService)
    {
        InitializeComponent();
        _soundService = soundService;

        // ดึงค่าปัจจุบันมาโชว์ในสวิตช์
        BgmSwitch.IsToggled = !_soundService.IsBgmMuted;
        SfxSwitch.IsToggled = !_soundService.IsSfxMuted;
        VolumeSlider.Value = _soundService.BgmVolume;
    }

    private void OnBgmToggled(object sender, ToggledEventArgs e)
    {
        if (_soundService.IsBgmMuted == e.Value)
        {
            _soundService.ToggleBgm();
        }
    }

    private void OnSfxToggled(object sender, ToggledEventArgs e)
    {
        if (_soundService.IsSfxMuted == e.Value)
        {
            _soundService.ToggleSfx();
        }
    }

    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        _soundService.SetBgmVolume(e.NewValue);
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        _soundService.PlayClickSound();
        await CloseAsync();
    }
}