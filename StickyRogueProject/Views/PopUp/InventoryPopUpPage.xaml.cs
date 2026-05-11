using StickyRogueProject.Models;
using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views.PopUp;

public partial class InventoryPopUpPage : ContentPage
{
    private readonly InventoryPopUpViewModel _viewModel;

    public InventoryPopUpPage(ActiveSave save)
        : this(save, new List<InventoryItem>()) { }

    public InventoryPopUpPage(ActiveSave save, List<string> newLootStrings)
        : this(save, newLootStrings.Select(InventoryItem.FromString).ToList()) { }

    public InventoryPopUpPage(ActiveSave save, List<InventoryItem> newLoot)
    {
        InitializeComponent();

        var saveService = IPlatformApplication.Current?.Services.GetService<SaveService>()
            ?? throw new InvalidOperationException("SaveService not found in DI container");

        _viewModel = new InventoryPopUpViewModel(save, newLoot, saveService)
        {
            ShowActionSheet = async (title, cancel, destroy, buttons) =>
                await DisplayActionSheet(title, cancel, destroy, buttons),

            ShowConfirm = async (title, message, accept, cancel) =>
                await DisplayAlert(title, message, accept, cancel),

            ShowAlert = async (title, message) =>
                await DisplayAlert(title, message, "OK"),

            ClosePopupAction = async () =>
                await Navigation.PopModalAsync()
        };

        BindingContext = _viewModel;
    }
}