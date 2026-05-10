// การเปลี่ยนแปลงใน Version นี้:
//   1. Win/Lose → Navigate ไป CombatPage อัตโนมัติหลังแสดงผล 1.8 วินาที
//   2. Draw → ให้กดซ้ำได้ ไม่ Navigate ออก (IsActionEnabled = true)
//   3. ลบ IsContinueVisible และปุ่ม "RETURN TO SHOP" ออก (ไม่จำเป็นแล้ว)
//   4. ลบ GoBackCommand (ไม่จำเป็นแล้ว — Navigation จัดการใน PlayRopAsync)
//   5. เพิ่ม IsResultVisible สำหรับแสดงข้อความผลลัพธ์ชั่วคราว

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using System.Text.Json;

namespace StickyRogueProject.ViewModels;

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
        new() { Id="sword_1",         Name="Iron Sword",        StatType="ATK",   StatBonus=1  },
        new() { Id="sword_2",         Name="Steel Blade",       StatType="ATK",   StatBonus=2  },
        new() { Id="sword_3",         Name="Demon Edge",        StatType="ATK",   StatBonus=3  },
        new() { Id="sword_4",         Name="Dragon Slayer",     StatType="ATK",   StatBonus=4  },
        new() { Id="shield_1",        Name="Wooden Buckler",    StatType="DEF",   StatBonus=1  },
        new() { Id="shield_2",        Name="Iron Shield",       StatType="DEF",   StatBonus=2  },
        new() { Id="shield_3",        Name="Knight Shield",     StatType="DEF",   StatBonus=3  },
        new() { Id="shield_4",        Name="Aegis",             StatType="DEF",   StatBonus=4  },
        new() { Id="tome_1",          Name="Apprentice Book",   StatType="INT",   StatBonus=1  },
        new() { Id="tome_2",          Name="Magic Scroll",      StatType="INT",   StatBonus=2  },
        new() { Id="tome_3",          Name="Necronomicon",      StatType="INT",   StatBonus=3  },
        new() { Id="ring_1",          Name="Ring of Power",     StatType="ATK",   StatBonus=2  },
        new() { Id="mana_crystal",    Name="Mana Crystal",      StatType="MAXMP", StatBonus=5  },
        new() { Id="sapphire_pendant",Name="Sapphire Pendant",  StatType="MAXMP", StatBonus=10 },
        new() { Id="ocean_soul",      Name="Ocean Soul",        StatType="MAXMP", StatBonus=20 },
    };

    public RopViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    // ── PlayRopCommand ───────────────────────────────────────
    // รับ playerChoice: "Rock", "Scissors", หรือ "Paper"
    // Win/Lose → แสดงผล → หน่วง 1.8 วินาที → Navigate ไป CombatPage
    // Draw     → แสดงผล → เปิดปุ่มคืน (ให้กดซ้ำได้)
    [RelayCommand]
    private async Task PlayRopAsync(string playerChoice)
    {
        // ปิดปุ่มกันกดรัวๆ ขณะประมวลผล
        IsActionEnabled = false;

        // สุ่ม CPU
        string[] choices = { "Rock", "Scissors", "Paper" };
        string cpuChoice = choices[_random.Next(0, 3)];

        // แสดงภาพที่เลือก
        UserChoiceImage = GetImageFileName(playerChoice);
        CpuChoiceImage = GetImageFileName(cpuChoice);
        IsVsVisible = true;

        // ตัดสินผล
        string result = DetermineWinner(playerChoice, cpuChoice);

        if (result == "DRAW")
        {
            // เสมอ — แสดงผล แล้วเปิดปุ่มให้กดซ้ำ ไม่ Navigate ออก
            ResultText = "DRAW! Try again.";
            ResultColor = Colors.DarkGray;
            IsActionEnabled = true;
            return;
        }

        // Win หรือ Lose — ใช้ await เพื่อให้บันทึกเสร็จก่อนแสดงผล
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

        // หน่วง 1.8 วินาทีให้ผู้เล่นเห็นผลลัพธ์ก่อน Navigate ออก
        await Task.Delay(1800);

        // Navigate กลับไป CombatPage โดยตรง (ข้ามร้านค้า)
        // ใช้ relative routing ".." → ".." เพื่อข้าม 2 หน้า (RopPage → ShopPage → CombatPage)
        await Shell.Current.GoToAsync("../..");
    }

    // ── HandleWinAsync ───────────────────────────────────────
    // สุ่ม Artifact รางวัล 1 ชิ้น เพิ่ม Level และ Stat บันทึก DB
    private async Task HandleWinAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save is null) return;

        // สุ่มของรางวัล
        var reward = _artifactPool[_random.Next(_artifactPool.Count)];

        // โหลด/สร้าง ArtifactData Dictionary
        var artifacts = new Dictionary<string, int>();
        if (!string.IsNullOrEmpty(save.ArtifactData))
            artifacts = JsonSerializer.Deserialize<Dictionary<string, int>>(save.ArtifactData)
                        ?? new Dictionary<string, int>();

        // เพิ่ม Level (สูงสุด 10)
        if (artifacts.ContainsKey(reward.Id))
            artifacts[reward.Id] = Math.Min(artifacts[reward.Id] + 1, 10);
        else
            artifacts[reward.Id] = 1;

        // บวก Stat
        ApplyStatBonus(save, reward.StatType, reward.StatBonus);

        // บันทึก
        save.ArtifactData = JsonSerializer.Serialize(artifacts);
        await _saveService.UpdateSaveAsync(save);

        // อัปเดตข้อความผลลัพธ์ให้แสดงชื่อรางวัล
        ResultText = $"WIN! 🎉 ได้รับ {reward.Name}\n(+{reward.StatBonus} {reward.StatType})";
    }

    // ── HandleLoseAsync ──────────────────────────────────────
    // สุ่มลด Stat 1 อย่าง (ATK / DEF / INT / MaxMP) บันทึก DB
    private async Task HandleLoseAsync()
    {
        var save = await _saveService.LoadSaveAsync();
        if (save is null) return;

        // สุ่ม 0-3 → ATK, DEF, INT, MaxMP (ไม่มี SPD)
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

        // อัปเดตข้อความผลลัพธ์ให้แสดง Stat ที่เสีย
        ResultText = $"LOSE! 💀 {statName} ลดลง 1";
    }

    // ── Helpers ──────────────────────────────────────────────

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
