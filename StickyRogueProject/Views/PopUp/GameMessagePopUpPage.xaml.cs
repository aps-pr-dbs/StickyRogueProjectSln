using System.Threading.Tasks;

namespace StickyRogueProject.Views.PopUp;

public partial class GameMessagePopUpPage : ContentPage
{
    // ⚡ สร้างตัวแปรเก็บตัวรับสัญญาณ
    private readonly TaskCompletionSource<bool> _tcs;

    // ⚡ เพิ่ม TaskCompletionSource<bool> tcs ในวงเล็บ
    public GameMessagePopUpPage(string title, string message, TaskCompletionSource<bool> tcs)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;

        _tcs = tcs; // เก็บค่าไว้ใช้ตอนกดปุ่ม
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        // 1. ปิดหน้าต่าง PopUp
        await Navigation.PopModalAsync();

        // 2. ⚡ ส่งสัญญาณบอก ViewModel ว่า "ผู้เล่นกดปุ่มตกลงแล้วนะ ไปต่อได้!"
        _tcs?.SetResult(true);
    }
}