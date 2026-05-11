using StickyRogueProject.Services;

namespace StickyRogueProject.Views.PopUp;

public partial class HistoryPopUp : ContentPage
{
    private readonly HistoryService _historyService;
    private bool _isClosing = false;
    public List<object> Histories { get; set; } // เปลี่ยน object เป็น Class History ของคุณอ๊าฟนะครับ

    public HistoryPopUp(HistoryService historyService)
    {
        InitializeComponent();
        _historyService = historyService;
        LoadHistory();
    }

    private async void LoadHistory()
    {
        // สมมติว่ามีฟังก์ชัน GetHistoryAsync ใน Service นะครับ
        // Histories = await _historyService.GetAllHistoryAsync();
        BindingContext = this;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (_isClosing) return;
        _isClosing = true;
        await Navigation.PopModalAsync();
    }
}