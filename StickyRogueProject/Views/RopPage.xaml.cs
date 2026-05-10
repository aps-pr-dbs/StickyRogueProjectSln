namespace StickyRogueProject.Views;

public partial class RopPage : ContentPage
{
    public RopPage(ViewModels.RopViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}