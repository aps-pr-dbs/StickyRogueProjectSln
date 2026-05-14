using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class EnemyStatusPopUpPage : ContentPage
{
    // ⚡ เพิ่มตัวแปรล็อกเพื่อกันการกดปุ่มเบิ้ล
    private bool _isClosing = false;

    public EnemyStatusPopUpPage(Enemy? enemy)
    {
        InitializeComponent();

        // Bind Enemy Model โดยตรง
        BindingContext = enemy;
    }

    // ปุ่ม Close
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // ⚡ ถ้ากำลังปิดอยู่ ให้หยุดทำงานทันที (กันมือลั่น)
        if (_isClosing) return;

        _isClosing = true; // ล็อกปุ่มไว้เลย!

        await Navigation.PopModalAsync();
    }
}