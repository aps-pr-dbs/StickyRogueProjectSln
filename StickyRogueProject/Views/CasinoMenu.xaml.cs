using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class CasinoMenu : ContentPage
{
    public CasinoMenu()
    {
        InitializeComponent();

        // ผูกหน้าจอกับ ViewModel
        BindingContext = new CasinoMenuViewModel();
    }
}