// การเปลี่ยนแปลงใน Version นี้:
//   1. แก้ Bug: _hasPurchasedThisRound เริ่มต้นที่ false (เดิมเป็น true — Bug)
//   2. เพิ่ม IsSoldOut property — XAML ใช้แสดง Banner + Disable ปุ่ม Buy
//   3. CanInteract() เช็ค !HasPurchasedThisRound เพิ่มเติม
//   4. หลังซื้อสำเร็จ 1 ครั้ง → HasPurchasedThisRound = true ล็อกทันที
//   5. InitializeAsync Reset HasPurchasedThisRound = false ทุกครั้งที่เข้าร้านใหม่

using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

public partial class ShopViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly Random _rng = new();
    private ActiveSave? _currentSave;
    private List<ArtifactItem> _masterPool = new();

    [ObservableProperty] private int _playerCoins;
    [ObservableProperty] private string _playerClass = string.Empty;
    [ObservableProperty] private string _merchantGreeting = "ยินดีต้อนรับ! เลือกสิ่งที่ต้องการได้เลย";
    [ObservableProperty] private ObservableCollection<ArtifactItem> _shopItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BuyArtifactCommand))]
    [NotifyCanExecuteChangedFor(nameof(MapsToRopCommand))]
    private bool _isBusy;

    [ObservableProperty] private string _feedbackMessage = string.Empty;
    [ObservableProperty] private bool _isFeedbackVisible;

    // ── Purchase Lock ────────────────────────────────────────
    // false = ยังไม่ได้ซื้อ → ซื้อได้
    // true  = ซื้อแล้ว 1 ชิ้น → ล็อกปุ่ม Buy ทุกปุ่ม
    // [NotifyPropertyChangedFor(nameof(IsSoldOut))] ทำให้ XAML รู้ว่า
    // IsSoldOut เปลี่ยนค่าพร้อมกับ HasPurchasedThisRound โดยอัตโนมัติ
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BuyArtifactCommand))]
    [NotifyPropertyChangedFor(nameof(IsSoldOut))]
    private bool _hasPurchasedThisRound = false;

    // IsSoldOut — Computed Property สำหรับ XAML Binding
    // ใช้แสดง/ซ่อน "Sold Out" Banner และ Disable ปุ่ม Buy จากนอก DataTemplate
    public bool IsSoldOut => _hasPurchasedThisRound;

    public IReadOnlyList<ArtifactItem> MasterPool => _masterPool.AsReadOnly();

    public ShopViewModel(SaveService saveService)
    {
        _saveService = saveService;
        _masterPool = BuildDefaultArtifacts();
    }

    // ── InitializeCommand ────────────────────────────────────
    // เรียกจาก OnAppearing ทุกครั้ง
    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            FeedbackMessage = string.Empty;

            // Reset Lock ทุกครั้งที่เข้าร้านใหม่
            // ถ้ากลับมาจาก RopPage หรือ Navigation อื่น จะได้ซื้อใหม่ได้
            HasPurchasedThisRound = false;

            _currentSave = await _saveService.LoadSaveAsync();
            if (_currentSave is null)
            {
                await Shell.Current.DisplayAlert("ข้อผิดพลาด", "ไม่พบข้อมูล Save", "ตกลง");
                await Shell.Current.GoToAsync("..");
                return;
            }

            PlayerCoins = _currentSave.Coins;
            PlayerClass = _currentSave.ClassName;

            RestoreArtifactLevels(_masterPool, _currentSave.ArtifactData);

            // สุ่ม 3 ชิ้นจาก Master Pool 15 ตัว
            var selected = _masterPool.OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
            ShopItems = new ObservableCollection<ArtifactItem>(selected);

            MerchantGreeting = _currentSave.Coins >= 50
                ? "ยินดีต้อนรับ! มีของดีมาให้เลือกวันนี้"
                : "เหรียญน้อยหน่อยนะ... แต่ก็มีของถูกให้เลือกเหมือนกัน";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShopViewModel] Initialize ล้มเหลว: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── BuyArtifactCommand ───────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task BuyArtifactAsync(ArtifactItem? artifact)
    {
        if (artifact is null || _currentSave is null) return;

        try
        {
            IsBusy = true;

            if (artifact.IsMaxLevel)
            {
                await ShowFeedbackAsync("Artifact นี้ถึง Max Level แล้ว! ✨");
                return;
            }

            if (_currentSave.Coins < artifact.Price)
            {
                await ShowFeedbackAsync($"เหรียญไม่พอ! ต้องการ {artifact.Price} 🪙");
                return;
            }

            // ── ซื้อสำเร็จ ──────────────────────────────────
            _currentSave.Coins -= artifact.Price;
            artifact.CurrentLevel += 1;

            ApplyStatBonus(_currentSave, artifact.StatType, artifact.StatBonus);

            _currentSave.ArtifactData = SerializeLevels(_masterPool);
            await _saveService.UpdateSaveAsync(_currentSave);

            PlayerCoins = _currentSave.Coins;

            // ล็อกร้านทันที — IsSoldOut = true โดยอัตโนมัติ
            // BuyArtifactCommand.CanExecute() จะคืน false ให้ทุกปุ่มพร้อมกัน
            HasPurchasedThisRound = true;

            MerchantGreeting = "ได้ของแล้ว! โชคดีในการต่อสู้ นักสู้! 👋";

            await ShowFeedbackAsync($"✅ ซื้อสำเร็จ! {artifact.Name} → Lv.{artifact.CurrentLevel}  (+{artifact.StatBonus} {artifact.StatType})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShopViewModel] ซื้อล้มเหลว: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── MapsToRopCommand ─────────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task MapsToRopAsync()
    {
        await Shell.Current.GoToAsync("RopPage");
    }

    // CanInteract: Busy = false AND ยังไม่ได้ซื้อในรอบนี้
    private bool CanInteract() => !IsBusy && !HasPurchasedThisRound;

    // ── Public Helpers สำหรับ Page อื่นเรียก ────────────────
    public ActiveSave? GetCurrentSave() => _currentSave;

    public void RefreshCoins()
    {
        if (_currentSave is not null) PlayerCoins = _currentSave.Coins;
    }

    public async Task SaveExternalAsync(ActiveSave save)
    {
        save.ArtifactData = SerializeLevels(_masterPool);
        await _saveService.UpdateSaveAsync(save);
    }

    // ── Private Helpers ──────────────────────────────────────

    private List<ArtifactItem> BuildDefaultArtifacts() => new()
    {
        new() { Key="atk_1", Name="Catfood Hammer",    Description="+2 ATK ต่อ Lv",       StatBonus=2,  StatType="ATK",   Price=25, ImageSource="catfood_hammer.png"      },
        new() { Key="atk_2", Name="Fishbone Sword",    Description="+3 ATK ต่อ Lv",       StatBonus=3,  StatType="ATK",   Price=40, ImageSource="fishbone_sword.png"      },
        new() { Key="atk_3", Name="Catlitter Blaster", Description="+5 ATK ต่อ Lv",       StatBonus=5,  StatType="ATK",   Price=70, ImageSource="catlitter_blaster.png"   },
        new() { Key="def_1", Name="Cardbox Armor",     Description="+2 DEF ต่อ Lv",       StatBonus=2,  StatType="DEF",   Price=25, ImageSource="cardbox_armor.png"       },
        new() { Key="def_2", Name="Laundry Helmet",    Description="+3 DEF ต่อ Lv",       StatBonus=3,  StatType="DEF",   Price=40, ImageSource="laundrybasket_helmet.png" },
        new() { Key="def_3", Name="Litterbox Armor",   Description="+5 DEF ต่อ Lv",       StatBonus=5,  StatType="DEF",   Price=70, ImageSource="litterbox_armor.png"     },
        new() { Key="int_1", Name="Noodle Compass",    Description="+2 INT ต่อ Lv",       StatBonus=2,  StatType="INT",   Price=25, ImageSource="noodle_compass.png"      },
        new() { Key="int_2", Name="Goldfish Staff",    Description="+3 INT ต่อ Lv",       StatBonus=3,  StatType="INT",   Price=40, ImageSource="goldfish_staff.png"      },
        new() { Key="int_3", Name="Human Tamer Tome",  Description="+5 INT ต่อ Lv",       StatBonus=5,  StatType="INT",   Price=70, ImageSource="humantamer_tome.png"     },
        new() { Key="hp_1",  Name="Catfood Backpack",  Description="+10 HP สูงสุด ต่อ Lv", StatBonus=10, StatType="HP",  Price=30, ImageSource="catfood_backpack.png"    },
        new() { Key="hp_2",  Name="King Meow Collar",  Description="+20 HP สูงสุด ต่อ Lv", StatBonus=20, StatType="HP",  Price=55, ImageSource="kingmeow_collar.png"    },
        new() { Key="hp_3",  Name="9 Lives Collar",    Description="+30 HP สูงสุด ต่อ Lv", StatBonus=30, StatType="HP",  Price=80, ImageSource="ninelives_collar.png"   },
        new() { Key="mp_1",  Name="Catwitch Hat",      Description="+5 Max MP ต่อ Lv",    StatBonus=5,  StatType="MAXMP", Price=25, ImageSource="catwitch_hat.png"        },
        new() { Key="mp_2",  Name="Goldfish Orb",      Description="+10 Max MP ต่อ Lv",   StatBonus=10, StatType="MAXMP", Price=40, ImageSource="goldfish_orb.png"        },
        new() { Key="mp_3",  Name="Ancient Meow Tome", Description="+20 Max MP ต่อ Lv",   StatBonus=20, StatType="MAXMP", Price=70, ImageSource="ancietmeow_tome.png"     },
    };

    private void RestoreArtifactLevels(List<ArtifactItem> pool, string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        try
        {
            var levels = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            if (levels is null) return;
            foreach (var item in pool)
                if (levels.TryGetValue(item.Key, out int lvl))
                    item.CurrentLevel = Math.Clamp(lvl, 0, item.MaxLevel);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShopViewModel] JSON Error: {ex.Message}");
        }
    }

    private string SerializeLevels(List<ArtifactItem> pool)
    {
        var dict = pool.ToDictionary(a => a.Key, a => a.CurrentLevel);
        return JsonSerializer.Serialize(dict);
    }

    internal static void ApplyStatBonus(ActiveSave save, string statType, int bonus)
    {
        switch (statType)
        {
            case "ATK": save.Atk += bonus; break;
            case "DEF": save.Def += bonus; break;
            case "INT": save.Int += bonus; break;
            case "HP": save.MaxHp += bonus; save.CurrentHp += bonus; break;
            case "MAXMP": save.MaxMp += bonus; save.CurrentMp += bonus; break;
        }
    }

    private async Task ShowFeedbackAsync(string message)
    {
        FeedbackMessage = message;
        IsFeedbackVisible = true;
        await Task.Delay(2500);
        IsFeedbackVisible = false;
    }
}
