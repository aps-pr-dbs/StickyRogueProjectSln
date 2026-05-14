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

        // ⚡ ดึงของขายทั้งหมดมาจาก Registry กลางที่เดียวจบ!
        _masterPool = ArtifactRegistry.GetAllArtifacts();
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

            // สุ่ม 3 ชิ้นจาก Master Pool
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

            // ⚡ เช็คว่ากระเป๋าเต็มไหม (Max 6 ช่อง)
            if (_currentSave.Inventory == null) _currentSave.Inventory = new List<InventoryArtifac>();
            if (_currentSave.Inventory.Count >= 6)
            {
                await ShowFeedbackAsync("กระเป๋าเต็ม! กรุณาจัดการไอเทมก่อน 🎒");
                return;
            }

            // ── ซื้อสำเร็จ (หักเงินอย่างเดียว ไม่บวก Stat) ──────────────────
            _currentSave.Coins -= artifact.Price;
            artifact.CurrentLevel += 1;

            // =======================================================
            // ⚡ โยนไอเทมเข้า "กระเป๋า (Inventory)" ⚡
            // =======================================================
            // ใช้ artifact.Key ในการดึงข้อมูลเพื่อแก้บัครูปกล่องของขวัญ
            var newItem = InventoryArtifac.FromString(artifact.Key);
            _currentSave.Inventory.Add(newItem);
            // =======================================================

            _currentSave.ArtifactData = SerializeLevels(_masterPool);
            await _saveService.UpdateSaveAsync(_currentSave);

            PlayerCoins = _currentSave.Coins;
            HasPurchasedThisRound = true;
            MerchantGreeting = "ได้ของแล้ว! โชคดีในการต่อสู้ นักสู้! 👋";

            await ShowFeedbackAsync($"✅ ซื้อสำเร็จ! {artifact.Name} ถูกเก็บลงกระเป๋า 🎒");
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