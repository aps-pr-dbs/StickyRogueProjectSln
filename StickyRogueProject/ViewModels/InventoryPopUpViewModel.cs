using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using System.Collections.ObjectModel;
using StickyRogueProject.ViewModels;

namespace StickyRogueProject.ViewModels;

// คลาสตัวแทนช่องเก็บของในกระเป๋า (1 ช่อง)
public partial class InventorySlotUI : ObservableObject
{
    public int Index { get; set; }
    public InventoryArtifac? Item { get; set; }

    [ObservableProperty] private string _icon = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _tag = string.Empty;
    [ObservableProperty] private Color _backgroundColor = Color.FromArgb("#F5F5F5");
    [ObservableProperty] private Color _borderColor = Color.FromArgb("#DDDDDD");
    [ObservableProperty] private bool _hasItem;
}

public partial class InventoryPopUpViewModel : ObservableObject
{
    // ปรับลดกระเป๋าเหลือ 6 ช่อง
    private const int MaxBagSlots = 6;

    // ช่องใส่ Artifact 6 ช่อง
    private const int MaxArtifactSlots = 6;

    private readonly ActiveSave _save;
    private readonly List<InventoryArtifac> _newLoot;
    private readonly SaveService _saveService;
    private bool IsBagFull => _save.Inventory?.Count >= MaxBagSlots;
    private bool IsArtifactsFull => _save.Artifacts?.Count >= MaxArtifactSlots;

    // ── Delegates สำหรับติดต่อกับ View ───────────────────
    public Func<string, string, string, string[], Task<string?>>? ShowActionSheet { get; set; }
    public Func<string, string, string, string, Task<bool>>? ShowConfirm { get; set; }
    public Func<string, string, Task>? ShowAlert { get; set; }
    public Func<Task>? ClosePopupAction { get; set; }

    // ── UI Properties ─────────────────────────────────────
    [ObservableProperty] private string _slotCountText = string.Empty;
    [ObservableProperty] private string _slotBadgeText = string.Empty;
    [ObservableProperty] private Color _slotBadgeColor = Colors.Transparent;

    [ObservableProperty] private bool _hasNewLoot;
    [ObservableProperty] private string _newDropIcon = string.Empty;
    [ObservableProperty] private string _newDropName = string.Empty;

    [ObservableProperty] private string _hintText = string.Empty;
    [ObservableProperty] private Color _hintTextColor = Colors.Gray;

    // Collections สำหรับแสดงผลใน XAML (6 Bag + 6 Artifact)
    public ObservableCollection<InventorySlotUI> BagSlots { get; } = new();
    public ObservableCollection<InventorySlotUI> ArtifactSlots { get; } = new();

    public InventoryPopUpViewModel(ActiveSave save, List<InventoryArtifac> newLoot, SaveService saveService)
    {
        _save = save;
        _newLoot = newLoot ?? new List<InventoryArtifac>();
        _saveService = saveService;

        _save.Inventory ??= new List<InventoryArtifac>();
        _save.Artifacts ??= new List<InventoryArtifac>(); // เพิ่ม List<InventoryItem> สำหรับ Artifacts ใน ActiveSave ด้วยนะครับ

        // กำหนดช่อง Bag 6 ช่อง
        for (int i = 0; i < MaxBagSlots; i++)
        {
            BagSlots.Add(new InventorySlotUI { Index = i });
        }

        // กำหนดช่อง Artifact 6 ช่อง
        for (int i = 0; i < MaxArtifactSlots; i++)
        {
            ArtifactSlots.Add(new InventorySlotUI { Index = i });
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        var inv = _save.Inventory;
        int used = inv.Count;

        // Header
        SlotCountText = $"{used} / {MaxBagSlots} slots used";
        SlotBadgeText = (MaxBagSlots - used).ToString();
        SlotBadgeColor = used switch
        {
            >= MaxBagSlots => Color.FromArgb("#CC0000"),
            >= MaxBagSlots - 2 => Color.FromArgb("#FF8C00"),
            _ => Color.FromArgb("#9ACD32")
        };

        // รีเฟรช Bag slots (6 ช่อง)
        for (int i = 0; i < MaxBagSlots; i++)
        {
            UpdateSlot(BagSlots[i], i < inv.Count ? inv[i] : null, false);
        }

        // รีเฟรช Artifact slots (6 ช่อง)
        var arts = _save.Artifacts;
        for (int i = 0; i < MaxArtifactSlots; i++)
        {
            UpdateSlot(ArtifactSlots[i], i < arts.Count ? arts[i] : null, true);
        }

        // New drop banner
        if (_newLoot.Count > 0)
        {
            // เช็คว่าไอเทมชิ้นนี้เป็น Coin หรือไม่ ถ้าเป็นให้รับเงินแล้วลบออกเลย
            if (_newLoot[0].Type == ItemType.Material && _newLoot[0].Name.Contains("Coin"))
            {
                // สมมติ Coin มี Value หรือเอาจาก Bonus ก็ได้
                _save.Coins += 10;
                _newLoot.RemoveAt(0);
                RefreshUI();
                return;
            }

            var drop = _newLoot[0];
            NewDropIcon = drop.Icon;
            NewDropName = drop.Name;
            HasNewLoot = true;

            if (!IsBagFull)
            {
                _save.Inventory.Add(_newLoot[0]);
                _newLoot.RemoveAt(0);
                HasNewLoot = false;
                RefreshUI();
                return;
            }

            HintText = "⚠️ Bag full! Tap an item to discard it";
            HintTextColor = Color.FromArgb("#CC0000");
        }
        else
        {
            HasNewLoot = false;
            HintText = inv.Count > 0 ? "Tap an item to use / equip / discard" : "No items yet";
            HintTextColor = Color.FromArgb("#AAAAAA");
        }
    }

    private static void UpdateSlot(InventorySlotUI slot, InventoryArtifac? item, bool isArtifactSlot)
    {
        slot.Item = item;
        slot.HasItem = item is not null;

        if (item is not null)
        {
            slot.Icon = item.Icon;
            slot.Name = item.Name;
            slot.Tag = GetItemTag(item);

            if (isArtifactSlot)
            {
                slot.BorderColor = Color.FromArgb("#F5C518");
                slot.BackgroundColor = Color.FromArgb("#FFFDE7");
            }
            else
            {
                slot.BorderColor = Colors.Transparent;
                slot.BackgroundColor = GetSlotColor(item);
            }
        }
        else
        {
            slot.Icon = isArtifactSlot ? "💠" : string.Empty;
            slot.Name = isArtifactSlot ? "Empty" : string.Empty;
            slot.Tag = string.Empty;
            slot.BorderColor = Color.FromArgb("#DDDDDD");
            slot.BackgroundColor = isArtifactSlot ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#F5F5F5");
        }
    }

    // ── Commands ──────────────────────────────────────────

    [RelayCommand]
    private async Task BagSlotTappedAsync(InventorySlotUI slot)
    {
        if (!slot.HasItem || slot.Item is null) return;
        if (ShowActionSheet is null) return;

        var item = slot.Item;
        var actions = new List<string>();

        if (item.IsUsable) actions.Add($"✅ Use  ({GetUsePreview(item)})");
        // เปลี่ยนเงื่อนไขการ Equip เป็นให้ใส่ได้เฉพาะ Artifact/Equipment
        if (item.IsEquipment) actions.Add("⚔️ Equip Artifact");
        actions.Add("🗑️ Discard");

        string? choice = await ShowActionSheet($"{item.Icon} {item.Name}", "Cancel", null, actions.ToArray());

        if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

        if (choice.StartsWith("✅ Use"))
        {
            bool ok = UseItem(item);
            if (ok)
            {
                _save.Inventory.Remove(item);
                if (_newLoot.Count > 0 && !IsBagFull)
                {
                    _save.Inventory.Add(_newLoot[0]);
                    _newLoot.RemoveAt(0);
                }
                await _saveService.UpdateSaveAsync(_save);
                RefreshUI();
            }
        }
        else if (choice.StartsWith("⚔️ Equip"))
        {
            if (IsArtifactsFull)
            {
                if (ShowAlert is not null)
                    await ShowAlert("⚠️ Artifacts Full", "You can only equip 6 Artifacts. Please unequip one first.");
                return;
            }

            EquipArtifact(item);
            await _saveService.UpdateSaveAsync(_save);
            RefreshUI();
        }
        else if (choice.StartsWith("🗑️ Discard") && ShowConfirm is not null)
        {
            bool confirm = await ShowConfirm("🗑️ Discard", $"Discard \"{item.Name}\"?", "Discard", "Cancel");
            if (!confirm) return;

            _save.Inventory.Remove(item);

            if (_newLoot.Count > 0 && !IsBagFull)
            {
                _save.Inventory.Add(_newLoot[0]);
                _newLoot.RemoveAt(0);
            }
            await _saveService.UpdateSaveAsync(_save);
            RefreshUI();
        }
    }

    [RelayCommand]
    private async Task ArtifactSlotTappedAsync(InventorySlotUI slot)
    {
        if (!slot.HasItem || slot.Item is null) return;
        if (ShowActionSheet is null) return;

        var item = slot.Item;
        var actions = new List<string> { "🔓 Unequip", "🗑️ Discard" };

        string? choice = await ShowActionSheet($"{item.Icon} {item.Name}", "Cancel", null, actions.ToArray());

        if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

        if (choice.StartsWith("🔓 Unequip"))
        {
            if (IsBagFull)
            {
                if (ShowAlert is not null)
                    await ShowAlert("⚠️ Bag Full", "Your inventory is full. Cannot unequip artifact.");
                return;
            }

            UnequipArtifact(item);
            await _saveService.UpdateSaveAsync(_save);
            RefreshUI();
        }
        else if (choice.StartsWith("🗑️ Discard") && ShowConfirm is not null)
        {
            bool confirm = await ShowConfirm("🗑️ Discard", $"Discard \"{item.Name}\"?\nStats will be reduced.", "Discard", "Cancel");
            if (!confirm) return;

            DiscardArtifact(item);
            await _saveService.UpdateSaveAsync(_save);
            RefreshUI();
        }
    }

    [RelayCommand]
    private async Task DiscardNewDropAsync()
    {
        if (_newLoot.Count == 0 || ShowConfirm is null) return;

        string itemName = _newLoot[0].Name;
        bool confirm = await ShowConfirm(
            "🗑️ Discard New Item",
            $"Discard \"{itemName}\"?\nYour existing inventory stays unchanged.",
            "Discard", "Keep");

        if (!confirm) return;

        _newLoot.RemoveAt(0);
        RefreshUI();
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        if (_newLoot.Count > 0)
        {
            if (ShowAlert is not null)
                await ShowAlert("⚠️ Pending Item", "Please decide what to do with the new item first.");
            return;
        }

        if (ClosePopupAction is not null)
            await ClosePopupAction();
    }

    // ── Logic ────────────────────────────────────────────

    private bool UseItem(InventoryArtifac item)
    {
        if (!item.IsUsable) return false;

        bool used = false;

        if (item.HpRestore > 0)
        {
            _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp + item.HpRestore);
            used = true;
        }

        if (item.MpRestore > 0)
        {
            _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp + item.MpRestore);
            used = true;
        }

        return used;
    }

    private void EquipArtifact(InventoryArtifac item)
    {
        if (!item.IsEquipment || IsArtifactsFull) return;

        _save.Inventory.Remove(item);
        _save.Artifacts.Add(item);

        // นำโบนัสมาบวกให้ตัวละคร
        _save.Atk += item.BonusAtk;
        _save.Def += item.BonusDef;
        _save.Int += item.BonusInt;
        _save.MaxHp += item.BonusMaxHp;
        _save.MaxMp += item.BonusMaxMp;
        // กรณี MaxHp เด้งขึ้น อาจจะบวกเลือดตามไปด้วย
        _save.CurrentHp += item.BonusMaxHp;
        _save.CurrentMp += item.BonusMaxMp;
    }

    private void UnequipArtifact(InventoryArtifac item)
    {
        if (IsBagFull) return;

        // Calculate total bonuses from all current artifacts
        int totalAtkBonus = _save.Artifacts.Sum(a => a.BonusAtk);
        int totalDefBonus = _save.Artifacts.Sum(a => a.BonusDef);
        int totalIntBonus = _save.Artifacts.Sum(a => a.BonusInt);
        int totalHpBonus = _save.Artifacts.Sum(a => a.BonusMaxHp);
        int totalMpBonus = _save.Artifacts.Sum(a => a.BonusMaxMp);

        // Calculate base stats (without any artifacts)
        int baseAtk = _save.Atk - totalAtkBonus;
        int baseDef = _save.Def - totalDefBonus;
        int baseInt = _save.Int - totalIntBonus;
        int baseMaxHp = _save.MaxHp - totalHpBonus;
        int baseMaxMp = _save.MaxMp - totalMpBonus;

        // Remove the artifact
        _save.Artifacts.Remove(item);

        // Recalculate bonuses without this artifact
        int newTotalAtkBonus = _save.Artifacts.Sum(a => a.BonusAtk);
        int newTotalDefBonus = _save.Artifacts.Sum(a => a.BonusDef);
        int newTotalIntBonus = _save.Artifacts.Sum(a => a.BonusInt);
        int newTotalHpBonus = _save.Artifacts.Sum(a => a.BonusMaxHp);
        int newTotalMpBonus = _save.Artifacts.Sum(a => a.BonusMaxMp);

        // Set stats to base + remaining bonuses (with minimum bounds)
        _save.Atk = Math.Max(1, baseAtk + newTotalAtkBonus);
        _save.Def = Math.Max(0, baseDef + newTotalDefBonus);
        _save.Int = Math.Max(1, baseInt + newTotalIntBonus);
        _save.MaxHp = Math.Max(10, baseMaxHp + newTotalHpBonus);
        _save.MaxMp = Math.Max(0, baseMaxMp + newTotalMpBonus);

        // Check if current HP/MP exceed max
        _save.CurrentHp = Math.Min(_save.CurrentHp, _save.MaxHp);
        _save.CurrentMp = Math.Min(_save.CurrentMp, _save.MaxMp);

        _save.Inventory.Add(item);
    }

    private void DiscardArtifact(InventoryArtifac item)
    {
        // Try to find artifact in Artifacts list (equipped)
        var artifactToRemove = _save.Artifacts.FirstOrDefault(a => a.Name == item.Name);

        // If not found, try Inventory list (unequipped)
        if (artifactToRemove is null)
        {
            artifactToRemove = _save.Inventory.FirstOrDefault(a => a.Name == item.Name);
        }

        // If still not found, exit
        if (artifactToRemove is null) return;

        // Check if the artifact is currently equipped
        bool isEquipped = _save.Artifacts.Contains(artifactToRemove);

        if (isEquipped)
        {
            // Calculate total bonuses from all current equipped artifacts
            int totalAtkBonus = _save.Artifacts.Sum(a => a.BonusAtk);
            int totalDefBonus = _save.Artifacts.Sum(a => a.BonusDef);
            int totalIntBonus = _save.Artifacts.Sum(a => a.BonusInt);
            int totalHpBonus = _save.Artifacts.Sum(a => a.BonusMaxHp);
            int totalMpBonus = _save.Artifacts.Sum(a => a.BonusMaxMp);

            // Calculate base stats (without any artifacts)
            int baseAtk = _save.Atk - totalAtkBonus;
            int baseDef = _save.Def - totalDefBonus;
            int baseInt = _save.Int - totalIntBonus;
            int baseMaxHp = _save.MaxHp - totalHpBonus;
            int baseMaxMp = _save.MaxMp - totalMpBonus;

            // Remove equipped artifact
            _save.Artifacts.Remove(artifactToRemove);

            // Recalculate bonuses without this artifact
            int newTotalAtkBonus = _save.Artifacts.Sum(a => a.BonusAtk);
            int newTotalDefBonus = _save.Artifacts.Sum(a => a.BonusDef);
            int newTotalIntBonus = _save.Artifacts.Sum(a => a.BonusInt);
            int newTotalHpBonus = _save.Artifacts.Sum(a => a.BonusMaxHp);
            int newTotalMpBonus = _save.Artifacts.Sum(a => a.BonusMaxMp);

            // Set stats to base + remaining bonuses (with minimum bounds)
            _save.Atk = Math.Max(1, baseAtk + newTotalAtkBonus);
            _save.Def = Math.Max(0, baseDef + newTotalDefBonus);
            _save.Int = Math.Max(1, baseInt + newTotalIntBonus);
            _save.MaxHp = Math.Max(10, baseMaxHp + newTotalHpBonus);
            _save.MaxMp = Math.Max(0, baseMaxMp + newTotalMpBonus);

            // Check if current HP/MP exceed max
            _save.CurrentHp = Math.Min(_save.CurrentHp, _save.MaxHp);
            _save.CurrentMp = Math.Min(_save.CurrentMp, _save.MaxMp);
        }
        else
        {
            // If unequipped, just remove from inventory without stat adjustment
            _save.Inventory.Remove(artifactToRemove);
        }
    }

    // ── Helper Methods ────────────────────────────────────
    private static Color GetSlotColor(InventoryArtifac item) => item.Type switch
    {
        ItemType.Consumable => Color.FromArgb("#E8F5E9"),
        ItemType.Accessory => Color.FromArgb("#F3E5F5"), // ให้ถือว่า Artifact คือ Accessory
        _ => Color.FromArgb("#E8E8E8"),
    };

    private static string GetItemTag(InventoryArtifac item)
    {
        var tags = new List<string>();
        if (item.HpRestore > 0) tags.Add($"HP+{item.HpRestore}");
        if (item.MpRestore > 0) tags.Add($"MP+{item.MpRestore}");
        if (item.BonusAtk > 0) tags.Add($"ATK+{item.BonusAtk}");
        if (item.BonusDef > 0) tags.Add($"DEF+{item.BonusDef}");
        if (item.BonusInt > 0) tags.Add($"INT+{item.BonusInt}");

        if (tags.Count == 0) return "Artifact";
        return string.Join(",", tags);
    }

    private static string GetUsePreview(InventoryArtifac item)
    {
        var parts = new List<string>();
        if (item.HpRestore > 0) parts.Add($"HP+{item.HpRestore}");
        if (item.MpRestore > 0) parts.Add($"MP+{item.MpRestore}");
        return parts.Count > 0 ? string.Join(", ", parts) : "?";
    }
}