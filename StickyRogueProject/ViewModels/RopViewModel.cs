using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using System.Text.Json;

namespace StickyRogueProject.ViewModels;

// คลาสจำลองข้อมูล Artifact (ถ้า Claude สร้างไว้ให้ใน ShopViewModel แล้ว สามารถใช้ร่วมกันได้เลย)
public class RopArtifactReward
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StatType { get; set; } = string.Empty;
    public int StatBonus { get; set; }
}

public partial class RopViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly Random _random = new();

    // ตัวแปรควบคุมหน้าจอ UI
    [ObservableProperty]
    private bool _isVsVisible;

    [ObservableProperty]
    private string _userChoiceImage = "question_mark.png";

    [ObservableProperty]
    private string _cpuChoiceImage = "question_mark.png";

    [ObservableProperty]
    private string _resultText = "Choose your move!";

    [ObservableProperty]
    private Color _resultColor = Color.FromArgb("#2196F3"); // สีฟ้าเริ่มต้น

    [ObservableProperty]
    private bool _isActionEnabled = true;

    [ObservableProperty]
    private bool _isContinueVisible;

    // คลัง Artifact 18 ชิ้น สำหรับสุ่มแจกฟรีตอนชนะ
    private readonly List<RopArtifactReward> _artifactPool = new()
    {
        new RopArtifactReward { Id = "sword_1", Name = "Iron Sword", StatType = "ATK", StatBonus = 1 },
        new RopArtifactReward { Id = "sword_2", Name = "Steel Blade", StatType = "ATK", StatBonus = 2 },
        new RopArtifactReward { Id = "sword_3", Name = "Demon Edge", StatType = "ATK", StatBonus = 3 },
        new RopArtifactReward { Id = "sword_4", Name = "Dragon Slayer", StatType = "ATK", StatBonus = 4 },
        new RopArtifactReward { Id = "shield_1", Name = "Wooden Buckler", StatType = "DEF", StatBonus = 1 },
        new RopArtifactReward { Id = "shield_2", Name = "Iron Shield", StatType = "DEF", StatBonus = 2 },
        new RopArtifactReward { Id = "shield_3", Name = "Knight Shield", StatType = "DEF", StatBonus = 3 },
        new RopArtifactReward { Id = "shield_4", Name = "Aegis", StatType = "DEF", StatBonus = 4 },
        new RopArtifactReward { Id = "tome_1", Name = "Apprentice Book", StatType = "INT", StatBonus = 1 },
        new RopArtifactReward { Id = "tome_2", Name = "Magic Scroll", StatType = "INT", StatBonus = 2 },
        new RopArtifactReward { Id = "tome_3", Name = "Necronomicon", StatType = "INT", StatBonus = 3 },
        new RopArtifactReward { Id = "ring_1", Name = "Ring of Power", StatType = "ATK", StatBonus = 2 },
        new RopArtifactReward { Id = "mana_crystal", Name = "Mana Crystal", StatType = "MAXMP", StatBonus = 5 },
        new RopArtifactReward { Id = "sapphire_pendant", Name = "Sapphire Pendant", StatType = "MAXMP", StatBonus = 10 },
        new RopArtifactReward { Id = "ocean_soul", Name = "Ocean Soul", StatType = "MAXMP", StatBonus = 20 }
    };

    public RopViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    [RelayCommand]
    private async Task PlayRopAsync(string playerChoice)
    {
        // ปิดปุ่มกันคนกดรัวๆ
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
            // เสมอให้กดใหม่ได้
            IsActionEnabled = true;
        }
        else if (result == "WIN")
        {
            ResultText = "YOU WIN!";
            ResultColor = Colors.Green;
            IsContinueVisible = true;
            await HandleWinAsync();
        }
        else
        {
            ResultText = "YOU LOSE!";
            ResultColor = Colors.Red;
            IsContinueVisible = true;
            await HandleLoseAsync();
        }
    }

    private async Task HandleWinAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save == null) return;

        // สุ่มของรางวัล 1 ชิ้น
        var reward = _artifactPool[_random.Next(_artifactPool.Count)];

        // อ่าน JSON เดิม
        var artifacts = new Dictionary<string, int>();
        if (!string.IsNullOrEmpty(save.ArtifactData))
        {
            artifacts = JsonSerializer.Deserialize<Dictionary<string, int>>(save.ArtifactData) ?? new Dictionary<string, int>();
        }

        // เพิ่ม Level ให้ Artifact
        if (artifacts.ContainsKey(reward.Id))
        {
            artifacts[reward.Id] = Math.Min(artifacts[reward.Id] + 1, 10); // ตันที่ Level 10
        }
        else
        {
            artifacts[reward.Id] = 1;
        }

        // บวก Stat
        ApplyStatBonus(save, reward.StatType, reward.StatBonus);

        // บันทึกกลับ
        save.ArtifactData = JsonSerializer.Serialize(artifacts);
        await _saveService.UpdateSaveAsync(save);

        await Shell.Current.DisplayAlert("🎉 JACKPOT!", $"คุณได้รับฟรี: {reward.Name} (+{reward.StatBonus} {reward.StatType})", "Awesome!");
    }

    private async Task HandleLoseAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save == null) return;

        // สุ่มลด Stat 1 อย่าง (0=ATK, 1=DEF, 2=INT, 3=MaxMP)
        int statToReduce = _random.Next(0, 4);
        string statName = "";

        switch (statToReduce)
        {
            case 0:
                save.Atk = Math.Max(0, save.Atk - 1);
                statName = "ATK";
                break;
            case 1:
                save.Def = Math.Max(0, save.Def - 1);
                statName = "DEF";
                break;
            case 2:
                save.Int = Math.Max(0, save.Int - 1);
                statName = "INT";
                break;
            case 3:
                save.MaxMp = Math.Max(0, save.MaxMp - 1);
                statName = "MaxMP";
                break;
        }

        await _saveService.UpdateSaveAsync(save);

        await Shell.Current.DisplayAlert("💀 CURSED!", $"คุณโดนคำสาป! {statName} ลดลง 1 หน่วย", "Damn...");
    }

    private void ApplyStatBonus(ActiveSave save, string statType, int bonus)
    {
        switch (statType)
        {
            case "ATK": save.Atk += bonus; break;
            case "DEF": save.Def += bonus; break;
            case "INT": save.Int += bonus; break;
            case "MAXMP":
                save.MaxMp += bonus;
                save.CurrentMp += bonus;
                break;
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

    [RelayCommand]
    private async Task GoBackAsync()
    {
        // กลับไปหน้าร้านค้า
        await Shell.Current.GoToAsync("..");
    }
}