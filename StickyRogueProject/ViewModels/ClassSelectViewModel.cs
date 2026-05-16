using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

public partial class ClassSelectViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly List<CharacterClass> _classList;
    private int _currentIndex = 0;

    [ObservableProperty] private string _className = string.Empty;
    [ObservableProperty] private string _classDescription = string.Empty;
    [ObservableProperty] private string _classImage = string.Empty;
    [ObservableProperty] private string _classEmoji = string.Empty;
    [ObservableProperty] private string _accentColor = "#7B4FBF";

    // สเตตัสปัจจุบันของ Class
    [ObservableProperty] private int _statAtk;
    [ObservableProperty] private int _statDef;
    [ObservableProperty] private int _statInt;
    [ObservableProperty] private int _statMp; 
    [ObservableProperty] private int _statHp;

    [ObservableProperty] private string _classCounter = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectClassCommand))]
    private bool _isSaving = false;

    public ClassSelectViewModel(SaveService saveService)
    {
        _saveService = saveService;

        _classList = new List<CharacterClass>
        {
            new CharacterClass
            {
                Name        = "Warrior",
                Description = "นักรบผู้แกร่งกล้า ทนทานและถล่มหนัก\nเหมาะสำหรับผู้เล่นที่ชอบบุกตรงๆ",
                ImageSource = "fighter.png",
                AccentColor = "#B34A2A",
                BaseAtk     = 16,
                BaseDef     = 10,
                BaseInt     = 4,
                BaseMaxMp   = 30, // MP เริ่มต้นของนักรบ
                BaseMaxHp   = 120
            },
            new CharacterClass
            {
                Name        = "Rogue",
                Description = "นักลอบเร้นที่ว่องไว โจมตีไวและแม่นยำ\nสำหรับผู้เล่นที่ชอบเล่น High Risk / High Reward",
                ImageSource = "therogue.png",
                AccentColor = "#2A7A4A",
                BaseAtk     = 15,
                BaseDef     = 6,
                BaseInt     = 12,
                BaseMaxMp   = 60, // MP เริ่มต้นของโร้ก
                BaseMaxHp   = 85
            },
            new CharacterClass
            {
                Name        = "Mage",
                Description = "จอมเวทย์ผู้ทรงพลัง สาดเวทย์ทำลายล้าง\nเหมาะสำหรับผู้เล่นที่ชอบ Strategy",
                ImageSource = "magecat.png",
                AccentColor = "#3A4AB0",
                BaseAtk     = 6,
                BaseDef     = 4,
                BaseInt     = 18,
                BaseMaxMp   = 120, // MP เริ่มต้นของจอมเวทย์
                BaseMaxHp   = 70
            }
        };

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var current = _classList[_currentIndex];

        ClassName = current.Name;
        ClassDescription = current.Description;
        ClassImage = current.ImageSource;
        ClassEmoji = current.ThemeEmoji;
        AccentColor = current.AccentColor;
        StatAtk = current.BaseAtk;
        StatDef = current.BaseDef;
        StatInt = current.BaseInt;
        StatMp = current.BaseMaxMp;
        StatHp = current.BaseMaxHp;

        ClassCounter = $"{_currentIndex + 1} / {_classList.Count}";
    }

    [RelayCommand]
    private void PreviousClass()
    {
        if (_currentIndex == 0) _currentIndex = _classList.Count - 1;
        else _currentIndex--;
        RefreshDisplay();
    }

    [RelayCommand]
    private void NextClass()
    {
        _currentIndex = (_currentIndex + 1) % _classList.Count;
        RefreshDisplay();
    }

    [RelayCommand(CanExecute = nameof(CanSelect))]
    private async Task SelectClassAsync()
    {
        try
        {
            IsSaving = true;
            var selectedClass = _classList[_currentIndex];

            // สร้าง Save ใหม่พร้อมกำหนดค่าพื้นฐานให้ตรงกับโมเดลปัจจุบัน
            var newSave = new ActiveSave
            {
                ClassName = selectedClass.Name,
                Level = 1,
                CurrentWave = 1,     // อัปเดตเป็น CurrentWave
                CurrentLoop = 1,
                MaxHp = selectedClass.BaseMaxHp,
                CurrentHp = selectedClass.BaseMaxHp,
                MaxMp = selectedClass.BaseMaxMp,      
                CurrentMp = selectedClass.BaseMaxMp,  
                Atk = selectedClass.BaseAtk,
                Def = selectedClass.BaseDef,
                Int = selectedClass.BaseInt,
                Coins = 0,
                // กำหนด List ว่างให้กระเป๋าเพื่อป้องกัน NullReference
                Inventory = new List<InventoryArtifac>(),
                Artifacts = new List<InventoryArtifac>()
            };

            await _saveService.CreateNewSaveAsync(newSave);
            await Shell.Current.GoToAsync("CombatPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ClassSelectViewModel] Error: {ex.Message}");
            await Shell.Current.DisplayAlert("ข้อผิดพลาด", $"ไม่สามารถเริ่มเกมได้: {ex.Message}", "ตกลง");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSelect() => !IsSaving;
}