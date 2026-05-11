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
            await Shell.Current.DisplayAlert("รอดตัวไป!", "ดีใจด้วย! คุณได้รับ ATK, DEF, INT +20 และ MAX HP, MAX MP +50  ", "OK");
            await CloseWithAnimation("DealerWin");
        }
        else
        {
            await Shell.Current.DisplayAlert("ดวงแตก...", "เสียใจด้วย , ค่า ATK, DEF, INT ของคุณลดลง -10 รวมไปถึง MAX HP, MAX MP ลดลง -20", "OK");
            await CloseWithAnimation("DealerLose");
        }
    }

    private async void OnRunAwayClicked(object sender, EventArgs e)
    {
        int escapeChance = new Random().Next(1, 5);
        if (escapeChance == 1)
        {
            RunAwayBtn.IsVisible = false;
            await Shell.Current.DisplayAlert("โดนกระโดดจับตัว!", "เขาบล็อคไม่ให้คุณหนี! คุณหนีไม่สำเร็จ. คุณถูกบังคับให้ต้องดื่มยา", "โอ้ ไม่!");
        }
        else
        {
            await Shell.Current.DisplayAlert("รอดตัวไป", "คุณหนีสำเร็จ!", "OK");
            await CloseWithAnimation("Escaped");
        }
    }
}