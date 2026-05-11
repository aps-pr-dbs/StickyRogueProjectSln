using StickyRogueProject.Models;
using System.Windows.Input;

namespace StickyRogueProject.ViewModels;

public class InGameCharacterStatusViewModel
{
    private readonly ActiveSave _save;
    private readonly Action _closeAction;

    // เพิ่มตัวแปรเก็บค่า XP
    private readonly int _currentXp;
    private readonly int _xpToNextLevel;

    // รับค่า XP เข้ามาด้วย
    public InGameCharacterStatusViewModel(ActiveSave save, int currentXp, int xpToNextLevel, Action closeAction)
    {
        _save = save;
        _currentXp = currentXp;
        _xpToNextLevel = xpToNextLevel;
        _closeAction = closeAction;

        CloseCommand = new Command(() => _closeAction?.Invoke());
    }

    public string Name => _save.ClassName;
    public string CharacterClass => _save.ClassName;
    public int Level => _save.Level;

    public int AttackPower => _save.Atk;
    public int Intelligence => _save.Int; // ใช้เป็น INT ล้วนๆ
    public int Defense => _save.Def;

    public string HpDisplay => $"{_save.CurrentHp} / {_save.MaxHp}";
    public string MpDisplay => $"{_save.CurrentMp} / {_save.MaxMp}";

    // ✅ เปลี่ยนมาใช้ค่าตัวเลขจริงที่รับเข้ามา
    public string XpDisplay => $"{_currentXp} / {_xpToNextLevel}";
    public double XpProgress => _xpToNextLevel > 0 ? Math.Clamp((double)_currentXp / _xpToNextLevel, 0, 1) : 0;

    public double HpProgress => _save.MaxHp > 0 ? Math.Clamp((double)_save.CurrentHp / _save.MaxHp, 0, 1) : 0;
    public double MpProgress => _save.MaxMp > 0 ? Math.Clamp((double)_save.CurrentMp / _save.MaxMp, 0, 1) : 0;

    public ICommand CloseCommand { get; }
}