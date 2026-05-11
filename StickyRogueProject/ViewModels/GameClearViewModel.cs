// ViewModels/GameClearViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StickyRogueProject.ViewModels;

public partial class GameClearViewModel : ObservableObject
{
    private readonly string[] _dialogs;
    private int _currentIndex = 0;

    [ObservableProperty]
    private string _currentDialogText;

    public GameClearViewModel(string className, int level, int coins)
    {
        _dialogs = new[]
        {
            "ในที่สุด... แสงสว่างก็ส่องประกายอีกครั้ง",
            $"ผู้กล้าผู้มีอาชีพ {className} ได้พิสูจน์ให้โลกเห็นแล้ว",
            "ร้อยด่าน ร้อยบทพิสูจน์ — ทุกอย่างผ่านมาด้วยสองมือของคุณเอง",
            "เงาแห่งความมืดมิดที่ปกคลุมดินแดนนี้มานานหลายศตวรรษ...",
            "ได้สลายหายไปพร้อมกับลมหายใจสุดท้ายของราชาปีศาจ",
            "ผู้คนต่างออกมาจากที่หลบซ่อน ร้องเพลงสดุดีนักรบผู้ยิ่งใหญ่",
            $"Lv.{level} — พลังที่คุณสะสมมาตลอดการเดินทาง คือพลังแห่งความหวัง",
            $"💰 เหรียญที่เหลือติดมือ {coins} เหรียญ — ร่องรอยแห่งการผจญภัย",
            "ตำนานบทนี้จะถูกจารึกไว้ตลอดกาล",
            "ขอบคุณที่ร่วมเดินทางมาด้วยกัน...",
            "🏆 THE END 🏆",
            "บันทึกประวัติ......"
        };
        CurrentDialogText = _dialogs[0];
    }

    [RelayCommand]
    private async Task NextDialogAsync()
    {
        _currentIndex++;
        if (_currentIndex < _dialogs.Length)
            CurrentDialogText = _dialogs[_currentIndex];
        else
            await GoToMainAsync();
    }

    [RelayCommand]
    private async Task SkipAsync() => await GoToMainAsync();

    private Task GoToMainAsync()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.MainPage = new AppShell();
        });
        return Task.CompletedTask;
    }
}