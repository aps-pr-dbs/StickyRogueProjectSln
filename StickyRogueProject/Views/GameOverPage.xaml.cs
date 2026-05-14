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
    //ป้องกันการกดปุ่ม Back (ย้อนกลับ) ของ Android / Windows
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}