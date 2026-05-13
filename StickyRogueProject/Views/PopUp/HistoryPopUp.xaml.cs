using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.Views.PopUp;

public partial class HistoryPopUp : ContentPage
{
    private readonly HistoryService _historyService;
    private bool _isClosing = false;

    // ⚡ เปลี่ยนจาก object เป็น RunHistory ให้ตรงกับ Model
    public List<RunHistory> Histories { get; set; } = new();

    public HistoryPopUp(HistoryService historyService)
    {
        InitializeComponent();
        _historyService = historyService;

        // ผูก UI ทันที
        BindingContext = this;
    }

    // ⚡ ใช้ OnAppearing เพื่อโหลดข้อมูลทุกครั้งที่เปิดหน้าต่างนี้
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        // ⚡ ดึงข้อมูลจากฐานข้อมูลมาใส่ใน List
        Histories = await _historyService.GetAllHistoryAsync();

        // ⚡ สั่งให้ UI อัปเดตหน้าจอเพื่อแสดงข้อมูลใหม่
        OnPropertyChanged(nameof(Histories));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (_isClosing) return;
        _isClosing = true;
        await Navigation.PopModalAsync();
    }
}