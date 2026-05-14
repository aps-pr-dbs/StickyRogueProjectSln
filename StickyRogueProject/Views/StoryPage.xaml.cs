using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class StoryPage : ContentPage
{
    public StoryPage(StoryViewModel viewModel)
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