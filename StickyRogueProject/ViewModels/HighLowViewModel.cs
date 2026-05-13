using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;

namespace StickyRogueProject.ViewModels;

// คลาสเก็บข้อมูลไพ่
public class PlayingCard
{
    public string Suit { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public int Value { get; set; }
    public Color CardColor { get; set; } = Colors.White;
}

public partial class HighLowViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private ActiveSave? _save;

    private List<PlayingCard> _deck = new();
    public PlayingCard? CurrentCard { get; private set; }
    public PlayingCard? NextCard { get; private set; }

    [ObservableProperty] private int _streakCount;
    [ObservableProperty] private string _streakText = "Streak : 0 / 3";
    [ObservableProperty] private Color _streakColor = Colors.White;

    // ⚡ ระบบ Betting
    [ObservableProperty] private int _coins;
    [ObservableProperty] private bool _isBettingPhase = true;
    [ObservableProperty] private int _currentBet = 0;
    [ObservableProperty] private string _dealerTaunt = "อยากรวยทางลัดงั้นรึ? วางเงินเดิมพันมาสิไอ้หนู!";
    [ObservableProperty] private string _statusText = "What you think about next card?";
    [ObservableProperty] private Color _statusColor = Colors.LightGray;

    [ObservableProperty] private bool _isGameOver;
    [ObservableProperty] private bool _isGameActive = false;

    public bool IsAnimating { get; set; }

    public Action<PlayingCard, PlayingCard>? OnInitCards { get; set; }
    public Func<PlayingCard, Task>? OnShowNextCard { get; set; }
    public Func<PlayingCard, PlayingCard, Task>? OnProceedToNextRound { get; set; }
    public Func<string, Task>? OnShowBlackScreenDialog { get; set; }

    // ⚡ ลบ CasinoRigService ออกจากตรงนี้ เพราะ Claude ทำเป็น Static ให้แล้ว
    public HighLowViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        _save = await _saveService.LoadSaveAsync();
        Coins = _save?.Coins ?? 0;
        IsBettingPhase = true;
        IsGameActive = false;
        IsGameOver = false;
        DealerTaunt = CasinoRigService.RandomOpeningTaunt();
    }

    [RelayCommand]
    public async Task PlaceBetAsync(string amountStr)
    {
        if (_save == null || !int.TryParse(amountStr, out int amount)) return;

        int actualBet = amount == -1 ? _save.Coins : amount; // -1 คือ All In

        if (actualBet <= 0 || _save.Coins < actualBet)
        {
            DealerTaunt = "ไม่มีเงินแล้วยังจะเสนอหน้ามาอีกเรอะ! ไปหาเงินมาก่อนไป!";
            return;
        }

        // หักเงินล่วงหน้าทันทีที่เล่น
        _save.Coins -= actualBet;
        Coins = _save.Coins;
        CurrentBet = actualBet;
        await _saveService.UpdateSaveAsync(_save);

        IsBettingPhase = false;
        StartNewGame();
    }

    private void StartNewGame()
    {
        IsGameOver = false;
        IsGameActive = true;
        StreakCount = 0;
        UpdateStreakUI();

        StatusText = "What you think about next card?";
        StatusColor = Colors.LightGray;
        DealerTaunt = "หึ... ขอให้ทายถูกละกันนะ";

        CreateDeck();
        CurrentCard = _deck[0]; _deck.RemoveAt(0);
        NextCard = _deck[0]; _deck.RemoveAt(0);

        OnInitCards?.Invoke(CurrentCard, NextCard);
    }

    private void CreateDeck()
    {
        _deck.Clear();
        string[] suits = { "♠", "♥", "♦", "♣" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        foreach (var suit in suits)
        {
            Color color = (suit == "♥" || suit == "♦") ? Colors.Red : Colors.Black;
            foreach (var rank in ranks)
            {
                int value = rank switch { "J" => 11, "Q" => 12, "K" => 13, "A" => 14, _ => int.Parse(rank) };
                _deck.Add(new PlayingCard { Suit = suit, Rank = rank, Value = value, CardColor = color });
            }
        }
        _deck = _deck.OrderBy(x => Random.Shared.Next()).ToList();
    }

    [RelayCommand]
    public async Task GuessAsync(string guess)
    {
        if (IsGameOver || IsAnimating || NextCard == null || CurrentCard == null) return;
        IsAnimating = true;

        bool guessHigh = guess == "High";
        if (OnShowNextCard != null) await OnShowNextCard(NextCard);

        await Task.Delay(800);

        bool isCorrect = (guessHigh && NextCard.Value > CurrentCard.Value) ||
                         (!guessHigh && NextCard.Value < CurrentCard.Value);

        if (isCorrect)
        {
            StreakCount++;
            UpdateStreakUI();

            if (StreakCount >= 3)
            {
                await HandleWinAsync();
            }
            else
            {
                StatusText = "You did well! Guess again.";
                StatusColor = Colors.LightGreen;
                CurrentCard = NextCard;
                NextCard = _deck[0]; _deck.RemoveAt(0);

                if (OnProceedToNextRound != null)
                    await OnProceedToNextRound(CurrentCard, NextCard);
            }
        }
        else
        {
            await HandleLossAsync();
        }

        IsAnimating = false;
    }

    // ⚡ อัปเดตให้รองรับ CasinoRigService โฉมใหม่ของ Claude
    private async Task HandleWinAsync()
    {
        if (_save == null) return;

        var (result, coinDelta, dealerLine, debuff) = CasinoRigService.ResolveWin(CurrentBet);

        _save.Coins += (CurrentBet + coinDelta);
        if (result == CasinoWinResult.Cheated && debuff.HasValue)
        {
            CasinoRigService.ApplyDebuff(_save, debuff.Value);
        }

        Coins = _save.Coins;
        await _saveService.UpdateSaveAsync(_save);
        DealerTaunt = dealerLine;

        if (result == CasinoWinResult.Cheated)
        {
            EndGame("CHEATED!", Colors.Red);
            if (OnShowBlackScreenDialog != null) await OnShowBlackScreenDialog(dealerLine);
        }
        else
        {
            EndGame(result == CasinoWinResult.DoubleWin ? "JACKPOT!" : "YOU WIN", Colors.Gold);
        }
    }

    // ⚡ อัปเดตให้รองรับ CasinoRigService โฉมใหม่ของ Claude
    private async Task HandleLossAsync()
    {
        if (_save == null) return;

        var (result, dealerLine) = CasinoRigService.ResolveLoss();

        if (result == CasinoLossResult.ItemStolen)
        {
            string stealMsg = CasinoRigService.StealItem(_save);
            dealerLine += $"\n(เสีย {stealMsg})";
        }

        Coins = _save.Coins;
        await _saveService.UpdateSaveAsync(_save);

        DealerTaunt = dealerLine;
        EndGame("YOU LOSE", Colors.Red);

        if (result == CasinoLossResult.ItemStolen)
        {
            if (OnShowBlackScreenDialog != null) await OnShowBlackScreenDialog(dealerLine);
        }
    }

    private void UpdateStreakUI()
    {
        StreakText = $"Streak : {StreakCount} / 3";
        StreakColor = (StreakCount == 0) ? Colors.White : Colors.LightGreen;
    }

    private void EndGame(string message, Color color)
    {
        IsGameOver = true;
        IsGameActive = false;
        IsBettingPhase = false; // กลับไปหน้าเลือกเดิมพัน
        StatusText = message;
        StatusColor = color;
    }
    // ⚡ คำสั่งสำหรับกดปุ่มย้อนกลับออกจากโต๊ะพนัน
    [RelayCommand]
    public async Task ExitGameAsync()
    {
        // สั่งให้ปิดหน้าต่างนี้ (PopModal) 
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
    
}