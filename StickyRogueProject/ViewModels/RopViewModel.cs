using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

// ⚡ ลบคลาส RopArtifactReward ทิ้งไปได้เลยครับ ไม่ต้องใช้แล้ว

public partial class RopViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly Random _random = new();

    [ObservableProperty] private bool _isVsVisible;
    [ObservableProperty] private string _userChoiceImage = "question_mark.png";
    [ObservableProperty] private string _cpuChoiceImage = "question_mark.png";
    [ObservableProperty] private string _resultText = "เลือกอาวุธของคุณเลย!";
    [ObservableProperty] private Color _resultColor = Color.FromArgb("#2196F3");

    [ObservableProperty] private string _shopkeeperDialogue = "กล้ามาเดิมพันกับข้าไหมล่ะ? หึหึหึ...";

    [ObservableProperty] private bool _isActionEnabled = true;
    [ObservableProperty] private bool _isGameOver = false;

    // ⚡ ลบ _artifactPool แบบเก่าที่ใช้ Emoji ทิ้งไปเช่นกัน

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
            ResultText = "DRAW!\nลองใหม่อีกครั้งสิ";
            ShopkeeperDialogue = "ใจตรงกันซะงั้น! เอาใหม่สิไอ้หนู!";
            ResultColor = Colors.DarkGray;
            IsActionEnabled = true;
            return;
        }

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

        // =======================================================
        // ⚡ สุ่มของรางวัลจากระบบ Artifact แท้ๆ (จะได้รูปภาพ .png มาด้วย)
        // =======================================================
        var reward = ArtifactRegistry.GetRandomArtifact();

        if (save.Inventory.Count < 6)
        {
            // ⚡ แปลง Key เป็น InventoryArtifac เพื่อดึงรูปมาให้ครบ
            var newItem = InventoryArtifac.FromString(reward.Key);
            save.Inventory.Add(newItem);

            ResultText = $"YOU WIN! 🎉\n\nได้รับ: {reward.Name}\n(+{reward.StatBonus} {reward.StatType})";
        }
        else
        {
            ApplyStatBonus(save, reward.StatType, reward.StatBonus);
            ResultText = $"YOU WIN! 🎉\n\nกระเป๋าเต็ม!\nดูดกลืนพลัง: +{reward.StatBonus} {reward.StatType}";
        }
        // =======================================================

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

    [RelayCommand]
    private async Task ExitGameAsync()
    {
        await Shell.Current.GoToAsync("../..");
    }
}