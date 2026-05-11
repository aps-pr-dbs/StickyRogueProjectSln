// Views/GameClearPage.xaml.cs
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class GameClearPage : ContentPage
{
    public GameClearPage(GameClearViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}