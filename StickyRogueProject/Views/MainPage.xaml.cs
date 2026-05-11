using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    // ⚡ เพิ่ม 2 ตัวนี้เข้ามา
    private readonly SaveService _saveService;
    private readonly HistoryService _historyService;

    // ⚡ รับ Service เข้ามาทาง Constructor (Dependency Injection)
    public MainPage(MainPageViewModel viewModel, SaveService saveService, HistoryService historyService)
    {
        InitializeComponent();

        BindingContext = viewModel;
        _viewModel = viewModel;
        _saveService = saveService;
        _historyService = historyService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckSaveStatusCommand.ExecuteAsync(null);
    }

    // เปิดหน้า Load Game
    private async void OnLoadGameBtnClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new PopUp.LoadGamePopUp(_saveService));
    }

    // เปิดหน้า History
    private async void OnHistoryBtnClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new PopUp.HistoryPopUp(_historyService));
    }
}