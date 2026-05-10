// รับ ActiveSave เข้ามาผ่าน Constructor
// แสดงข้อมูล Inventory Slots และ Potion Counts
// ปิดด้วย PopModalAsync

using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class InventoryPopUpPage : ContentPage
{
    public InventoryPopUpPage(ActiveSave? save)
    {
        InitializeComponent();

        // BindingContext ผูกกับ ActiveSave โดยตรง
        // XAML สามารถ Bind Slot1-Slot6, HpPotionCount, MpPotionCount ได้ทันที
        BindingContext = save;
    }

    // ปุ่ม Close — ปิด Modal กลับไป CombatPage
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
