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
    [ObservableProperty] private string _resultText = "เลือกอาวุธของคุณเลย!";
    [ObservableProperty] private Color _resultColor = Color.FromArgb("#2196F3");

    // ⚡ เพิ่มบทพูดของพ่อค้า
    [ObservableProperty] private string _shopkeeperDialogue = "กล้ามาเดิมพันกับข้าไหมล่ะ? หึหึหึ...";

    // ควบคุมปุ่มเป่ายิ้งฉุบ
    [ObservableProperty] private bool _isActionEnabled = true;

    // ⚡ ควบคุมปุ่มกดออก (จะโผล่มาตอนรู้ผลแพ้ชนะแล้ว)
    [ObservableProperty] private bool _isGameOver = false;

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

        // ⚡ จุดสำคัญ: เปลี่ยนให้ `IsVsVisible = true` เพื่อให้รูปค้างอยู่หน้าจอ
        IsVsVisible = true;

        string result = DetermineWinner(playerChoice, cpuChoice);

        if (result == "DRAW")
        {
            ResultText = "DRAW!\nลองใหม่อีกครั้งสิ";
            ShopkeeperDialogue = "ใจตรงกันซะงั้น! เอาใหม่สิไอ้หนู!";
            ResultColor = Colors.DarkGray;

            // ⚡ เมื่อเสมอ เราเปิดปุ่มเป่ายิ้งฉุบให้กดได้ใหม่ทันที
            IsActionEnabled = true;

            // ⚡ เราจะไม่ปิด `IsVsVisible = false` ที่นี่ เพื่อให้รูปคู่ HAMMER VS SCISSORS ค้างอยู่
            return;
        }

        // --- ส่วนของผลแพ้หรือชนะ (คุณอ๊าฟมีอยู่แล้ว) ---
        // (เมื่อผลออกมาแพ้/ชนะ โค้ดส่วนนี้จะทำงานต่อและปิดรับเป่ายิ้งฉุบ)
        if (result == "WIN")
        {
            ResultColor = Colors.LimeGreen;
            ShopkeeperDialogue = "หนอยแน่แก... ไอ้คนชั่ว!\nฝากไว้ก่อนเถอะ!";
            await HandleWinAsync();
        }
        else
        {
            ResultColor = Colors.OrangeRed;
            ShopkeeperDialogue = "ชิ! แกบังอาจจะมาปล้นฉันหรอ\nทำอะไรก็ควรได้รับผลแบบนั้นแหละนะ หึหึหึ";
            await HandleLoseAsync();
        }

        IsGameOver = true;
    }

    private async Task HandleWinAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save is null) return;

        save.Inventory ??= new List<InventoryArtifac>();
        var reward = _artifactPool[_random.Next(_artifactPool.Count)];

        var newItem = new InventoryArtifac
        {
            Name = reward.Name,
            Icon = reward.Icon,
            Type = ItemType.Accessory,
            Description = $"+{reward.StatBonus} {reward.StatType}"
        };

        switch (reward.StatType)
        {
            case "ATK": newItem.BonusAtk = reward.StatBonus; break;
            case "DEF": newItem.BonusDef = reward.StatBonus; break;
            case "INT": newItem.BonusInt = reward.StatBonus; break;
            case "MAXMP": newItem.BonusMaxMp = reward.StatBonus; break;
        }

        if (save.Inventory.Count < 6)
        {
            save.Inventory.Add(newItem);
            // ⚡ จัดบรรทัดใหม่ให้อ่านง่าย
            ResultText = $"YOU WIN! 🎉\n\nได้รับ: {reward.Name}\n(+{reward.StatBonus} {reward.StatType})";
        }
        else
        {
            ApplyStatBonus(save, reward.StatType, reward.StatBonus);
            // ⚡ จัดบรรทัดใหม่ให้อ่านง่าย
            ResultText = $"YOU WIN! 🎉\n\nกระเป๋าเต็ม!\nดูดกลืนพลัง: +{reward.StatBonus} {reward.StatType}";
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
        // ⚡ จัดบรรทัดใหม่ให้อ่านง่าย
        ResultText = $"YOU LOSE! 💀\n\nโดนสาป: {statName} ลดลง 1";
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

    // ⚡ ปุ่มกดออก (ทำงานเมื่อคลิก)
    [RelayCommand]
    private async Task ExitGameAsync()
    {
        await Shell.Current.GoToAsync("../..");
    }
}