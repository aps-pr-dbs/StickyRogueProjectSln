// ViewModels/GameOverViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StickyRogueProject.ViewModels;

public partial class GameOverViewModel : ObservableObject
{
    private readonly string[] _dialogs;
    private int _currentIndex = 0;

    [ObservableProperty]
    private string _currentDialogText;

    public GameOverViewModel(string enemyName, int loop, int wave)
    {
        _dialogs = new[]
        {
            $"คุณล้มลงด้วยน้ำมือของ {enemyName}...",
            $"Loop {loop}, Wave {wave} — ความมืดมิดกลืนกินคุณไปในที่สุด",
            "เสียงแห่งความหวังดับสลายลงพร้อมกับลมหายใจสุดท้ายของคุณ...",
            "แม้เรื่องราวของผู้กล้าคนหนึ่งจะสิ้นสุดลง",
            "แต่เปลวไฟแห่งโชคชะตายังคงไม่มอดดับ",
            "ตราบใดที่โลกยังจมอยู่ในความมืดมิด...",
            "ผู้กล้าคนใหม่ก็จะถือกำเนิดขึ้นอีกครั้ง",
            "และเรื่องราวบทใหม่... กำลังรอคอยการเริ่มต้น",
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