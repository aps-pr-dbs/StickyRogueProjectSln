// Code-Behind ทำหน้าที่เพียง:
//   1. รับ ViewModel ผ่าน Dependency Injection
//   2. ผูก BindingContext
//   3. จัดการปุ่ม Back ที่ต้อง Navigate กลับ (เหตุผล: Shell.NavBarIsVisible="False")

using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class ClassSelectPage : ContentPage
{
    // Constructor รับ ViewModel จาก DI Container
    public ClassSelectPage(ClassSelectViewModel viewModel)
    {
        InitializeComponent();

        // ผูก ViewModel กับ Page
        BindingContext = viewModel;
    }

    // OnBackClicked — จัดการการกดปุ่ม Back ที่วาดเองใน XAML
    // เนื่องจากซ่อน NavBar ไว้ (Shell.NavBarIsVisible="False")
    // จึงต้องวาดปุ่ม Back เองและเรียก GoToAsync จาก Code-Behind
    // การ Navigate กลับไม่ถือเป็น Business Logic จึงทำได้ในที่นี้
    private async void OnBackClicked(object sender, EventArgs e)
    {
        // ".." หมายถึงย้อนกลับไปหน้าก่อนหน้าใน Navigation Stack
        await Shell.Current.GoToAsync("..");
    }
}
