using CommunityToolkit.Maui.Views;

namespace StickyRogueProject.Views.PopUp;

public partial class CasinoEventPopUpPage : Popup
{
    // ⚡ เพิ่มตัวล็อก
    private bool _isClosing = false;

    public CasinoEventPopUpPage()
    {
        InitializeComponent();
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        // เด้งดึ๋งตอนเปิด
        await CardContainer.ScaleTo(1, 500, Easing.SpringOut);
    }

    private async void OnEnterClicked(object sender, EventArgs e)
    {
        // ⚡ กันคนกดเข้าคาสิโนรัวๆ
        if (_isClosing) return;
        _isClosing = true;

        // หดลงก่อนปิด
        await CardContainer.ScaleTo(0, 200, Easing.SpringIn);
        await CloseAsync();
        // ปิดเสร็จระบบหน้าจอหลักน่าจะพาเด้งไป CasinoMenu ให้เองตามที่เขียนไว้ครับ
    }
}