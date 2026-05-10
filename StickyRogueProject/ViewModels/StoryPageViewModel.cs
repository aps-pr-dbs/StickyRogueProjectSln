using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StickyRogueProject.ViewModels;

public partial class StoryViewModel : ObservableObject
{
    private readonly string[] _dialogs = new string[]
    {
        "ในโลกที่ความมืดมิดได้กลืนกินแสงสว่างไปจนหมดสิ้น...",
        "มอนสเตอร์ออกอาละวาด ผู้คนต่างสูญเสียความหวัง",
        "แต่คำทำนายโบราณได้กล่าวถึงผู้กล้าที่จะลุกขึ้นสู้",
        "ผู้ที่จะนำพลังแห่งแสงสว่างกลับคืนมายังดินแดนนี้อีกครั้ง",
        "และการเดินทางของคุณ... กำลังจะเริ่มต้นขึ้น ณ บัดนี้"
    };

    private int _currentDialogIndex = 0;

    // ⚡ ข้อความที่จะแสดงบนหน้าจอ (ผูก Binding ไว้)
    [ObservableProperty]
    private string _currentDialogText;

    public StoryViewModel()
    {
        // เริ่มต้นด้วยข้อความแรก
        CurrentDialogText = _dialogs[_currentDialogIndex];
    }

    // ⚡ คำสั่งเมื่อผู้ใช้แตะหน้าจอ
    [RelayCommand]
    private async Task NextDialogAsync()
    {
        _currentDialogIndex++;

        if (_currentDialogIndex < _dialogs.Length)
        {
            CurrentDialogText = _dialogs[_currentDialogIndex];
        }
        else
        {
            await GoToClassSelectAsync();
        }
    }

    // ⚡ คำสั่งเมื่อกดปุ่ม Skip
    [RelayCommand]
    private async Task SkipAsync()
    {
        await GoToClassSelectAsync();
    }

    private async Task GoToClassSelectAsync()
    {
        // ย้ายไปหน้าเลือกคลาส (ต้อง RegisterRoute ใน AppShell ไว้แล้ว)
        await Shell.Current.GoToAsync("ClassSelectPage");
    }
}