using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.Views;

public partial class MainPage : ContentPage
{
    private bool _isSoundMuted = false;
    private readonly MainPageViewModel _viewModel;

    // ⚡ เพิ่ม 2 ตัวนี้เข้ามา
    private readonly SaveService _saveService;
    private readonly HistoryService _historyService;
    private readonly SoundService _soundService;
    // ⚡ รับ Service เข้ามาทาง Constructor (Dependency Injection)
    public MainPage(MainPageViewModel viewModel, SaveService saveService, HistoryService historyService, SoundService soundService)
    {
        InitializeComponent();

        BindingContext = viewModel;
        _viewModel = viewModel;
        _saveService = saveService;
        _historyService = historyService;

        // ⚡ 3. เก็บค่าไว้ในตัวแปร
        _soundService = soundService;
    }
    private void OnToggleSoundClicked(object sender, EventArgs e)
    {
        // เล่นเสียงปุ่มกด
        _soundService.PlayClickSound();

        // ⚡ สลับสถานะเปิด/ปิดเสียง และจัดการ BGM ภายใน Service เลย
        _soundService.ToggleMute();

        // เปลี่ยนไอคอนหรือข้อความบนปุ่มตามสถานะล่าสุด
        if (_soundService.IsMuted)
        {
            SoundToggleBtn.Text = "🔇";
            // SoundToggleBtn.Source = "embed://speaker_off_icon.png"; // ถ้าใช้รูปภาพ
        }
        else
        {
            SoundToggleBtn.Text = "🔊";
            // SoundToggleBtn.Source = "embed://speaker_icon.png"; // ถ้าใช้รูปภาพ
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CheckSaveStatusCommand.ExecuteAsync(null);
    }

    // เปิดหน้า Load Game
    private async void OnLoadGameBtnClicked(object sender, EventArgs e)
    {
        // ⚡ สั่งเล่นเสียงผ่าน _soundService
        _soundService.PlayClickSound();

        await Navigation.PushModalAsync(new PopUp.LoadGamePopUp(_saveService));
    }

    // เปิดหน้า History
    private async void OnHistoryBtnClicked(object sender, EventArgs e)
    {
        // ⚡ สั่งเล่นเสียงผ่าน _soundService
        _soundService.PlayClickSound();

        await Navigation.PushModalAsync(new PopUp.HistoryPopUp(_historyService));
    }
    private void OnStartGameBtnClicked(object sender, EventArgs e)
    {
        // ⚡ สั่งเล่นเสียงผ่าน _soundService
        _soundService.PlayClickSound();
    }
}