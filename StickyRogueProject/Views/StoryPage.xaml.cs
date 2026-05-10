using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class StoryPage : ContentPage
{
    public StoryPage(StoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}