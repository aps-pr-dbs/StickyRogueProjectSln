using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

// ⚡ ตัวช่วยสำหรับโชว์ในกระเป๋า
public partial class InventorySlotUI : ObservableObject
{
    [ObservableProperty] private string _icon = "";
    [ObservableProperty] private string _name = "Empty";
    [ObservableProperty] private string _tag = "";
    [ObservableProperty] private string _backgroundColor = "#2A1D48";
    [ObservableProperty] private string _borderColor = "Transparent";

    public Models.InventoryArtifac? Item { get; set; }
    public bool IsEmpty => Item == null;
}

public partial class InventoryPopUpViewModel : ObservableObject
{
    private readonly ActiveSave _save;
    private readonly SaveService _saveService;

    // ⚡ ตัวส่งคำสั่งไปปิดหน้าต่าง
    public Action? ClosePopupAction { get; set; }

    [ObservableProperty] private ObservableCollection<InventorySlotUI> _artifactSlots = new();
    [ObservableProperty] private ObservableCollection<InventorySlotUI> _bagSlots = new();
    [ObservableProperty] private string _slotCountText = "0 / 6 slots used";
    [ObservableProperty] private string _slotBadgeText = "0";
    [ObservableProperty] private string _hintText = "เลือกไอเทมเพื่อสวมใส่หรือถอด";

    public InventoryPopUpViewModel(ActiveSave save, SaveService saveService)
    {
        _save = save;
        _saveService = saveService;
        RefreshUi();
    }

    private void RefreshUi()
    {
        if (_save.Artifacts == null) _save.Artifacts = new System.Collections.Generic.List<InventoryArtifac>();
        if (_save.Inventory == null) _save.Inventory = new System.Collections.Generic.List<InventoryArtifac>();

        ArtifactSlots.Clear();
        for (int i = 0; i < 6; i++)
        {
            if (i < _save.Artifacts.Count)
            {
                var item = _save.Artifacts[i];
                ArtifactSlots.Add(new InventorySlotUI { Name = item.Name, Icon = item.Icon, Item = item, BorderColor = "#A07ED0" });
            }
            else ArtifactSlots.Add(new InventorySlotUI { Name = "Empty", Icon = "", Item = null });
        }

        BagSlots.Clear();
        for (int i = 0; i < 6; i++)
        {
            if (i < _save.Inventory.Count)
            {
                var item = _save.Inventory[i];
                BagSlots.Add(new InventorySlotUI { Name = item.Name, Icon = item.Icon, Item = item });
            }
            else BagSlots.Add(new InventorySlotUI { Name = "Empty", Icon = "", Item = null });
        }

        SlotCountText = $"{_save.Inventory.Count} / 6 slots used";
        SlotBadgeText = _save.Inventory.Count.ToString();
    }

    // ===============================================================
    // ⚡ ส่วนที่แก้ใหม่: เมื่อกดไอเทมในกระเป๋า (รอสวมใส่)
    // ===============================================================
    [RelayCommand]
    private async Task BagSlotTappedAsync(InventorySlotUI slot)
    {
        if (slot.IsEmpty || slot.Item == null) return;

        // 1. สร้างตัวดักรอสัญญาณ
        var tcs = new TaskCompletionSource<string>();

        // 2. เรียก PopUp หน้าตาเท่ๆ ของเรา (ส่ง false ไปเพราะยังไม่ได้ใส่)
        await App.Current.MainPage!.Navigation.PushModalAsync(new Views.PopUp.InventoryActionPopUpPage(slot.Name, false, tcs));

        // 3. รอจนกว่าผู้เล่นจะกดปุ่มใดปุ่มหนึ่งบน PopUp
        string action = await tcs.Task;

        // 4. ทำงานตามที่ผู้เล่นเลือก
        if (action == "Equip")
        {
            if (_save.Artifacts.Count >= 6) { HintText = "❌ ช่องสวมใส่เต็มแล้ว!"; return; }
            var item = slot.Item;
            _save.Inventory.Remove(item);
            _save.Artifacts.Add(item);
            UpdatePlayerStats(item, isEquipping: true);
            await SaveAndRefresh("✅ สวมใส่สำเร็จ! Stat เพิ่มขึ้นแล้ว");
        }
        else if (action == "Discard")
        {
            _save.Inventory.Remove(slot.Item);
            await SaveAndRefresh("🗑️ ทิ้งไอเทมเรียบร้อย");
        }
    }

    // ===============================================================
    // ⚡ ส่วนที่แก้ใหม่: เมื่อกดไอเทมที่สวมใส่อยู่ (รอถอดออก)
    // ===============================================================
    [RelayCommand]
    private async Task ArtifactSlotTappedAsync(InventorySlotUI slot)
    {
        if (slot.IsEmpty || slot.Item == null) return;

        // 1. สร้างตัวดักรอสัญญาณ
        var tcs = new TaskCompletionSource<string>();

        // 2. เรียก PopUp (ส่ง true ไปเพราะใส่อยู่ ปุ่มจะเปลี่ยนเป็น "ถอดออก")
        await App.Current.MainPage!.Navigation.PushModalAsync(new Views.PopUp.InventoryActionPopUpPage(slot.Name, true, tcs));

        // 3. รอจนกว่าผู้เล่นจะกดปุ่ม
        string action = await tcs.Task;

        // 4. ทำงานตามที่ผู้เล่นเลือก
        if (action == "Unequip")
        {
            if (_save.Inventory.Count >= 6) { HintText = "❌ กระเป๋าเต็ม! ถอดไม่ได้"; return; }
            var item = slot.Item;
            _save.Artifacts.Remove(item);
            _save.Inventory.Add(item);
            UpdatePlayerStats(item, isEquipping: false);
            await SaveAndRefresh("✅ ถอดออกแล้ว Stat ลดลงตามปกติ");
        }
        else if (action == "Discard")
        {
            _save.Artifacts.Remove(slot.Item);
            UpdatePlayerStats(slot.Item, isEquipping: false);
            await SaveAndRefresh("🗑️ ทิ้งไอเทมเรียบร้อย");
        }
    }

    private void UpdatePlayerStats(InventoryArtifac item, bool isEquipping)
    {
        int multiplier = isEquipping ? 1 : -1;
        _save.Atk += item.BonusAtk * multiplier;
        _save.Def += item.BonusDef * multiplier;
        _save.Int += item.BonusInt * multiplier;
        _save.MaxHp += item.BonusMaxHp * multiplier;
        _save.MaxMp += item.BonusMaxMp * multiplier;
        _save.CurrentHp = Math.Clamp(_save.CurrentHp, 0, _save.MaxHp);
        _save.CurrentMp = Math.Clamp(_save.CurrentMp, 0, _save.MaxMp);
    }

    private async Task SaveAndRefresh(string message)
    {
        await _saveService.UpdateSaveAsync(_save);
        RefreshUi();
        HintText = message;
    }

    [RelayCommand]
    private void Close()
    {
        ClosePopupAction?.Invoke();
    }
}