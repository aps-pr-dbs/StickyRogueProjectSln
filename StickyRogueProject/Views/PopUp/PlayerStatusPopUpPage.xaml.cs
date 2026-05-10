// รับ ActiveSave เข้ามาผ่าน Constructor
// แสดง Stats ทั้งหมด: ATK, DEF, INT, HP, MP, Coins, Level
// ไม่มี SPD (ถูกลบออกแล้ว)

using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class PlayerStatusPopUpPage : ContentPage
{
    public PlayerStatusPopUpPage(ActiveSave? save)
    {
        InitializeComponent();

        // Bind ActiveSave โดยตรง — XAML ใช้ {Binding Atk}, {Binding Def} ฯลฯ
        BindingContext = save;
    }

    // ปุ่ม Close
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
