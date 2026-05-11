using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

// คลาสเก็บข้อมูลรางวัลในมินิเกม
public class RopArtifactReward
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string StatType { get; set; } = string.Empty;
    public int StatBonus { get; set; }
}

public partial class RopViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly Random _random = new();

    [ObservableProperty] private bool _isVsVisible;
    [ObservableProperty] private string _userChoiceImage = "question_mark.png";
    [ObservableProperty] private string _cpuChoiceImage = "question_mark.png";
    [ObservableProperty] private string _resultText = "Choose your move!";
    [ObservableProperty] private Color _resultColor = Color.FromArgb("#2196F3");

    // IsActionEnabled — ปิดปุ่มขณะประมวลผล
    // Draw = เปิดคืน, Win/Lose = ปิดจนกว่าจะ Navigate ออก
    [ObservableProperty] private bool _isActionEnabled = true;

    // คลัง Artifact รางวัลเมื่อชนะ
    private readonly List<RopArtifactReward> _artifactPool = new()
    {
        new() { Name="Iron Sword",        Icon="🗡️", StatType="ATK",   StatBonus=1  },
        new() { Name="Steel Blade",       Icon="⚔️", StatType="ATK",   StatBonus=2  },
        new() { Name="Demon Edge",        Icon="🗡️", StatType="ATK",   StatBonus=3  },
        new() { Name="Wooden Buckler",    Icon="🛡️", StatType="DEF",   StatBonus=1  },
        new() { Name="Iron Shield",       Icon="🛡️", StatType="DEF",   StatBonus=2  },
        new() { Name="Apprentice Book",   Icon="📖", StatType="INT",   StatBonus=1  },
        new() { Name="Magic Scroll",      Icon="📜", StatType="INT",   StatBonus=2  },
        new() { Name="Mana Crystal",      Icon="🔮", StatType="MAXMP", StatBonus=5  },
    };

    public RopViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    [RelayCommand]
    private async Task PlayRopAsync(string playerChoice)
    {
        IsActionEnabled = false;

        string[] choices = { "Rock", "Scissors", "Paper" };
        string cpuChoice = choices[_random.Next(0, 3)];

        UserChoiceImage = GetImageFileName(playerChoice);
        CpuChoiceImage = GetImageFileName(cpuChoice);
        IsVsVisible = true;

        string result = DetermineWinner(playerChoice, cpuChoice);

        if (result == "DRAW")
        {
            ResultText = "DRAW! Try again.";
            ResultColor = Colors.DarkGray;
            IsActionEnabled = true;
            return;
        }

        if (result == "WIN")
        {
            ResultText = "YOU WIN! 🎉";
            ResultColor = Colors.LimeGreen;
            await HandleWinAsync();
        }
        else
        {
            ResultText = "YOU LOSE! 💀";
            ResultColor = Colors.OrangeRed;
            await HandleLoseAsync();
        }

        await Task.Delay(1800);
        await Shell.Current.GoToAsync("../..");
    }

    private async Task HandleWinAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save is null) return;

        // ดึงกระเป๋ามาใช้งาน
        save.Inventory ??= new List<InventoryItem>();

        // สุ่มของรางวัล
        var reward = _artifactPool[_random.Next(_artifactPool.Count)];

        // สร้างเป็น Item จริงๆ เพื่อใส่เข้ากระเป๋าผู้เล่น
        var newItem = new InventoryItem
        {
            Name = reward.Name,
            Icon = reward.Icon,
            Type = ItemType.Accessory, // ให้ Artifact ถือว่าเป็น Accessory ไปก่อน
            Description = $"+{reward.StatBonus} {reward.StatType}"
        };

        // แจก Stat โบนัส
        switch (reward.StatType)
        {
            case "ATK": newItem.BonusAtk = reward.StatBonus; break;
            case "DEF": newItem.BonusDef = reward.StatBonus; break;
            case "INT": newItem.BonusMagic = reward.StatBonus; break;
            case "MAXMP": newItem.BonusMaxMp = reward.StatBonus; break;
        }

        // ใส่ของลงในช่องเก็บของ ถ้ากระเป๋ายังไม่เต็ม
        if (save.Inventory.Count < 6)
        {
            save.Inventory.Add(newItem);
            ResultText = $"WIN! 🎉 ได้รับ {reward.Name}\n(+{reward.StatBonus} {reward.StatType})";
        }
        else
        {
            // ถ้ากระเป๋าเต็มก็ให้แค่ Stat เปล่าๆ ไม่ต้องให้ไอเทม
            ApplyStatBonus(save, reward.StatType, reward.StatBonus);
            ResultText = $"WIN! 🎉 Bag Full! ดูดกลืนพลัง: +{reward.StatBonus} {reward.StatType}";
        }

        await _saveService.UpdateSaveAsync(save);
    }

    private async Task HandleLoseAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save is null) return;

        int statToReduce = _random.Next(0, 4);
        string statName;

        switch (statToReduce)
        {
            case 0: save.Atk = Math.Max(0, save.Atk - 1); statName = "ATK"; break;
            case 1: save.Def = Math.Max(0, save.Def - 1); statName = "DEF"; break;
            case 2: save.Int = Math.Max(0, save.Int - 1); statName = "INT"; break;
            default: save.MaxMp = Math.Max(0, save.MaxMp - 1); statName = "MaxMP"; break;
        }

        await _saveService.UpdateSaveAsync(save);
        ResultText = $"LOSE! 💀 {statName} ลดลง 1";
    }

    private void ApplyStatBonus(ActiveSave save, string statType, int bonus)
    {
        switch (statType)
        {
            case "ATK": save.Atk += bonus; break;
            case "DEF": save.Def += bonus; break;
            case "INT": save.Int += bonus; break;
            case "MAXMP": save.MaxMp += bonus; save.CurrentMp += bonus; break;
        }
    }

    private string GetImageFileName(string choice) => choice switch
    {
        "Rock" => "hammer.png",
        "Scissors" => "scissors.png",
        "Paper" => "paper.png",
        _ => "question_mark.png"
    };

    private string DetermineWinner(string user, string cpu)
    {
        if (user == cpu) return "DRAW";
        if ((user == "Rock" && cpu == "Scissors") ||
            (user == "Scissors" && cpu == "Paper") ||
            (user == "Paper" && cpu == "Rock")) return "WIN";
        return "LOSE";
    }
}