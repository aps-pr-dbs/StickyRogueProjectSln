// รับ Enemy Model เข้ามาผ่าน Constructor
// แสดง Stats ของศัตรู: Name, Level, HP, ATK, DEF, INT

using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class EnemyStatusPopUpPage : ContentPage
{
    public EnemyStatusPopUpPage(Enemy? enemy)
    {
        InitializeComponent();

        // Bind Enemy Model โดยตรง
        BindingContext = enemy;
    }

    // ปุ่ม Close
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
