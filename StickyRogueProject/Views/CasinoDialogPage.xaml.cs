namespace StickyRogueProject.Views;

public partial class CasinoDialogPage : ContentPage
{
    public CasinoDialogPage(string message)
    {
        InitializeComponent();
        lblMessage.Text = message; // เอาคำด่ามาใส่ Label
    }

    protected override bool OnBackButtonPressed() => true;

    private async void BtnContinue_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}