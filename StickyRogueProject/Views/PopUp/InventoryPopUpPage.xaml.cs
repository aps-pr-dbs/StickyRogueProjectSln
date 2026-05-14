using CommunityToolkit.Maui.Views;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views.PopUp;

public partial class InventoryPopUpPage : Popup
{
    public InventoryPopUpPage(InventoryPopUpViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        // ⚡ ผูกระบบปิดหน้าต่างเข้ากับคำสั่งใน ViewModel
        viewModel.ClosePopupAction = async () =>
        {
            await CloseAsync();
        };
    }
}