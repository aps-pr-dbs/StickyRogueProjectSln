using CommunityToolkit.Maui.Views;

namespace StickyRogueProject.Views.PopUp;

public partial class CasinoEventPopUpPage : Popup
{
    public CasinoEventPopUpPage()
    {
        InitializeComponent();
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        // เด้งดึ๋งตอนเปิด
        await CardContainer.ScaleTo(1, 500, Easing.SpringOut);
    }

    private async void OnEnterClicked(object sender, EventArgs e)
    {
        // หดลงก่อนปิด
        await CardContainer.ScaleTo(0, 200, Easing.SpringIn);
        await CloseAsync();
    }
}