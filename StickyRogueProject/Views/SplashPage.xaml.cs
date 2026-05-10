namespace StickyRogueProject.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติทันทีที่หน้านี้แสดงขึ้นมา
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. หน่วงเวลาให้ Lottie Animation เล่นสัก 3 วินาที (3000 มิลลิวินาที)
        await Task.Delay(3000);

        // 2. หมดเวลาปุ๊บ สั่งวาร์ปไปหน้าหลัก (MainPage) 
        await Shell.Current.GoToAsync("MainPage");
    }
}