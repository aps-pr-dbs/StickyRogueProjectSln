using System;
using System.Threading.Tasks;

namespace StickyRogueProject.Views.PopUp;

public partial class GameMessagePopUpPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs;

    // ⚡ 1. สร้างตัวแปรล็อกกันการกดเบิ้ล
    private bool _isClosing = false;

    public GameMessagePopUpPage(string title, string message, TaskCompletionSource<bool> tcs)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        _tcs = tcs;
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        // ถ้ากำลังปิดอยู่ (กดซ้ำ) ให้ดีดออกทันที
        if (_isClosing) return;
        _isClosing = true;

        // ⚡ 1. สั่งปิดหน้าต่าง PopUp ให้เสร็จสมบูรณ์ก่อน! (สำคัญมาก)
        await Navigation.PopModalAsync();

        // ⚡ 2. ปิดเสร็จปุ๊บ ค่อยส่งสัญญาณบอกให้เกมโหลดหน้าร้านค้าขึ้นมา
        _tcs?.TrySetResult(true);
    }

    // ⚡ 4. ป้องกันผู้เล่นกดปุ่ม Back (ย้อนกลับ) ที่ตัวเครื่อง เพื่อหนีหน้าต่างนี้
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}