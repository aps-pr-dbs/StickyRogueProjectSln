using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

public partial class ChurchViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private ActiveSave _save;

    [ObservableProperty] private string _playerName = string.Empty;
    [ObservableProperty] private int _currentLoop = 1;
    [ObservableProperty] private string _hpDisplay = string.Empty;
    [ObservableProperty] private string _mpDisplay = string.Empty;
    [ObservableProperty] private int _coins;
    [ObservableProperty] private string _churchMessage = "ขอให้โชคดีในการเดินทาง นักสู้";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAndExitCommand))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private bool _isBusy;

    public ChurchViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    private bool CanInteract() => !IsBusy;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            _save = await _saveService.LoadSaveAsync();
            if (_save != null)
            {
                _save.CurrentHp = _save.MaxHp;
                _save.CurrentMp = _save.MaxMp;
                await _saveService.UpdateSaveAsync(_save);

                PlayerName = _save.ClassName;
                CurrentLoop = _save.CurrentLoop;
                Coins = _save.Coins;
                HpDisplay = $"{_save.CurrentHp} / {_save.MaxHp}";
                MpDisplay = $"{_save.CurrentMp} / {_save.MaxMp}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task SaveAndExitAsync()
    {
        IsBusy = true;
        await Shell.Current.GoToAsync("MainPage");
        IsBusy = false;
    }

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task ContinueAsync()
    {
        if (_save == null) return;
        IsBusy = true;

        _save.CurrentLoop++;
        _save.CurrentWave = 1;
        await _saveService.UpdateSaveAsync(_save);

        await Shell.Current.GoToAsync("CombatPage");
        IsBusy = false;
    }
}