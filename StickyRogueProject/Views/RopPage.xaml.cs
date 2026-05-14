namespace StickyRogueProject.Views;

public partial class RopPage : ContentPage
{
    public RopPage(ViewModels.RopViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    //ป้องกันการกดปุ่ม Back (ย้อนกลับ) ของ Android / Windows
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}