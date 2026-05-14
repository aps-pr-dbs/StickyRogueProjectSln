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
    //ป้องกันการกดปุ่ม Back (ย้อนกลับ) ของ Android / Windows
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}