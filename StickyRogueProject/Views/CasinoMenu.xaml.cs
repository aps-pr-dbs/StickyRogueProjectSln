namespace StickyRogueProject.Views;

public partial class CasinoMenu : ContentPage
{
    public CasinoMenu()
    {
        InitializeComponent();
    }

    private async void BtnBlackjack_Clicked(object sender, EventArgs e)
    {
        // ใช้ PushAsync เพื่อเปิดหน้าเกม Blackjack (มีปุ่ม Back กลับมาหน้าเมนูได้)
        await Navigation.PushModalAsync(new BlackjackPage());
    }

    private async void BtnHighLow_Clicked(object sender, EventArgs e)
    {
        // ผมใส่แจ้งเตือนไว้ก่อน เพราะเรายังไม่ได้สร้างหน้า HighLowPage ครับ
        // ถ้าสร้างเสร็จแล้ว ค่อยเอาคอมเมนต์บรรทัดล่างออกครับ
        // await Navigation.PushAsync(new HighLowPage());

        await Navigation.PushModalAsync(new HighLowPage());
    }
    private async void BtnExit_Clicked(object sender, EventArgs e)
    {
        // สั่งวาร์ปกลับไปลุยหน้าต่อสู้ต่อ (รัน Wave ถัดไป)
        await Shell.Current.GoToAsync("//CombatPage");
    }
}