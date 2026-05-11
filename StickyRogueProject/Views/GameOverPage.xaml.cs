// Views/GameOverPage.xaml.cs
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class GameOverPage : ContentPage
{
    public GameOverPage(GameOverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}