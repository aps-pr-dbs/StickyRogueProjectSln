using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.Views.PopUp;

public partial class LoadGamePopUp : ContentPage
{
    private readonly SaveService _saveService;
    private bool _isActionInProgress = false;
    public ActiveSave? SaveData { get; set; }
    public bool HasSave => SaveData != null;
    public bool HasNoSave => SaveData == null;

    public LoadGamePopUp(SaveService saveService)
    {
        InitializeComponent();
        _saveService = saveService;
        LoadSaveInfo();
    }

    private async void LoadSaveInfo()
    {
        SaveData = await _saveService.LoadSaveAsync();
        BindingContext = this;
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        if (_isActionInProgress) return;
        _isActionInProgress = true;

        await Navigation.PopModalAsync();

        await Shell.Current.GoToAsync(nameof(Views.CombatPage));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (_isActionInProgress) return;
        _isActionInProgress = true;
        await Navigation.PopModalAsync();
    }
}