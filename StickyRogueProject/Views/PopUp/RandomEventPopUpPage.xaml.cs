using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class RandomEventPopUpPage : Popup
{
    public string ResultAction { get; set; } = string.Empty;

    public RandomEventPopUpPage(GameEvent gameEvent)
    {
        InitializeComponent();
        BindingContext = gameEvent;
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        await CardContainer.ScaleTo(1, 500, Easing.SpringOut);
    }

    private async Task CloseWithAnimation(string action)
    {
        ResultAction = action;
        await CardContainer.ScaleTo(0, 200, Easing.SpringIn);
        await CloseAsync();
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        await CloseWithAnimation("Normal");
    }

    private async void OnAcceptDealerClicked(object sender, EventArgs e)
    {
        int outcome = new Random().Next(0, 2);
        if (outcome == 1)
        {
            await Shell.Current.DisplayAlert("Success!", "Congratulations! You gained +5 to all stats", "OK");
            await CloseWithAnimation("DealerWin");
        }
        else
        {
            await Shell.Current.DisplayAlert("Failure...", "Unfortunately, you lost -5 to all stats", "OK");
            await CloseWithAnimation("DealerLose");
        }
    }

    private async void OnRunAwayClicked(object sender, EventArgs e)
    {
        int escapeChance = new Random().Next(1, 5);
        if (escapeChance == 1)
        {
            RunAwayBtn.IsVisible = false;
            await Shell.Current.DisplayAlert("Caught!", "He blocked your way! You can't escape. You must accept his deal.", "Oh no!");
        }
        else
        {
            await Shell.Current.DisplayAlert("Safe", "You ran away safely!", "OK");
            await CloseWithAnimation("Escaped");
        }
    }
}