using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using System.Collections.ObjectModel;

namespace StickyRogueProject.ViewModels;

public partial class BlackjackViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private ActiveSave? _save;

    private List<PlayingCard> _deck = new();
    public List<PlayingCard> PlayerHand { get; private set; } = new();
    public List<PlayingCard> DealerHand { get; private set; } = new();

    [ObservableProperty] private int _playerMatchScore;
    [ObservableProperty] private int _dealerMatchScore;
    [ObservableProperty] private string _playerScoreText = "Your : 0";
    [ObservableProperty] private string _dealerScoreText = "Dealer : ?";
    [ObservableProperty] private string _statusText = "Your Turn";
    [ObservableProperty] private Color _statusColor = Colors.White;
    [ObservableProperty] private string _restartButtonText = "Next round";

    // ⚡ Betting System
    [ObservableProperty] private int _coins;
    [ObservableProperty] private bool _isBettingPhase = true;
    [ObservableProperty] private int _currentBet = 0;
    [ObservableProperty] private string _dealerTaunt = "อยากวัดดวงงั้นรึ? วางเงินมาเลย!";

    [ObservableProperty] private bool _isGameOver;
    [ObservableProperty] private bool _isGameActive = false;
    [ObservableProperty] private bool _isRestartButtonVisible = false; // ⚡ Controls when restart button is shown

    private int _lastRoundWinner = 0; // ⚡ Track who won the last round (1=player, 2=dealer, 0=push)

    // ⚡ Delegates
    public Func<List<PlayingCard>, bool, bool, Task>? OnHitCard { get; set; }
    public Action? OnClearBoard { get; set; }
    public Action<int, int>? OnUpdateMatchDots { get; set; }
    public Action? OnFlipDealerCard { get; set; }
    public Func<string, Task>? OnShowBlackScreenDialog { get; set; }

    public BlackjackViewModel(SaveService saveService)
    {
        _saveService = saveService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        _save = await _saveService.LoadSaveAsync();
        Coins = _save?.Coins ?? 0;

        if (IsGameOver || PlayerMatchScore >= 3 || DealerMatchScore >= 3)
        {
            return;
        }
        IsBettingPhase = true;
        IsGameActive = false;
        IsGameOver = false;
        IsRestartButtonVisible = false; // ⚡ ซ่อนปุ่ม Restart ไว้ตอนเริ่มโต๊ะใหม่
        DealerTaunt = CasinoRigService.RandomOpeningTaunt();
    }

    [RelayCommand]
    public async Task PlaceBetAsync(string amountStr)
    {
        // ⚡ Check if match is already over
        if (PlayerMatchScore >= 3 || DealerMatchScore >= 3)
        {
            DealerTaunt = "เกมจบแล้ว! กดปุ่ม Reset Match เพื่อเล่นใหม่!";
            return;
        }

        if (_save == null || !int.TryParse(amountStr, out int amount)) return;

        int actualBet = amount == -1 ? _save.Coins : amount;

        if (actualBet <= 0 || _save.Coins < actualBet)
        {
            DealerTaunt = "เงินไม่พอ! อย่ามาทำตัวกระจอกแถวนี้!";
            return;
        }

        _save.Coins -= actualBet;
        Coins = _save.Coins;
        CurrentBet = actualBet;
        await _saveService.UpdateSaveAsync(_save);

        IsBettingPhase = false;
        await DealCardsAsync();
    }

    // ⚡ เมื่อกดปุ่ม Next Round / Reset
    [RelayCommand]
    public void StartNewGame()
    {
        bool isMatchReset = PlayerMatchScore >= 3 || DealerMatchScore >= 3;

        // ⚡ Hide restart button when starting new game
        IsRestartButtonVisible = false;

        // ⚡ Path 1: Match reset (someone reached 3 points) - show betting phase
        if (isMatchReset)
        {
            PlayerMatchScore = 0;
            DealerMatchScore = 0;
            OnUpdateMatchDots?.Invoke(0, 0);
            IsBettingPhase = true; // ⚡ Only show betting after match reset
            StatusText = "วางเงินเดิมพัน";
            DealerTaunt = "ใหม่อีกครั้ง... เอาเงินมาวางซะ!";
        }
        else
        {
            // ⚡ Path 2: Next round within same match - skip betting, deal directly
            IsBettingPhase = false;
            StatusText = "Your Turn";
            DealerTaunt = "รอบต่อไป... เอาไพ่!";
        }

        IsGameOver = false;
        StatusColor = Colors.White;

        PlayerHand.Clear();
        DealerHand.Clear();
        OnClearBoard?.Invoke();

        PlayerScoreText = "Your : 0";
        DealerScoreText = "Dealer : ?";

        // ⚡ If not in betting phase, directly deal cards for next round
        if (!IsBettingPhase)
        {
            _ = DealCardsDirectlyAsync();
        }
    }

    // ⚡ Helper method to deal cards without betting phase
    private async Task DealCardsDirectlyAsync()
    {
        IsGameOver = false;
        IsGameActive = true;
        StatusText = "Your Turn";
        StatusColor = Colors.White;
        DealerTaunt = "หึ... แจกไพ่!";

        CreateDeck();

        await DrawCardAsync(PlayerHand, false, false);
        await DrawCardAsync(DealerHand, true, true);
        await DrawCardAsync(PlayerHand, false, false);
        await DrawCardAsync(DealerHand, false, true);

        UpdateScores(false);

        if (CalculateScore(PlayerHand) == 21)
        {
            await DetermineWinnerAsync();
        }
    }

    // ⚡ เริ่มแจกไพ่หลังวางเงินเสร็จ
    private async Task DealCardsAsync()
    {
        IsGameOver = false;
        IsGameActive = true;
        StatusText = "Your Turn";
        StatusColor = Colors.White;
        DealerTaunt = "หึ... แจกไพ่!";

        CreateDeck();

        await DrawCardAsync(PlayerHand, false, false);
        await DrawCardAsync(DealerHand, true, true);
        await DrawCardAsync(PlayerHand, false, false);
        await DrawCardAsync(DealerHand, false, true); // ⚡ แก้แล้ว: isDealer = true (ไพ่ไม่บินไปหาผู้เล่นแล้ว)

        UpdateScores(false);

        if (CalculateScore(PlayerHand) == 21)
        {
            await DetermineWinnerAsync();
        }
    }

    private async Task DrawCardAsync(List<PlayingCard> hand, bool isHidden, bool isDealer)
    {
        if (_deck.Count == 0) return;

        var card = _deck[0];
        _deck.RemoveAt(0);
        hand.Add(card);

        if (OnHitCard != null)
            await OnHitCard(hand, isHidden, isDealer);
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
                int val = rank switch { "J" or "Q" or "K" => 10, "A" => 11, _ => int.Parse(rank) };
                _deck.Add(new PlayingCard { Suit = suit, Rank = rank, Value = val, CardColor = color });
            }
        }
        _deck = _deck.OrderBy(x => Random.Shared.Next()).ToList();
    }

    [RelayCommand]
    public async Task HitAsync()
    {
        if (IsGameOver) return;

        await DrawCardAsync(PlayerHand, false, false);
        UpdateScores(false);

        if (CalculateScore(PlayerHand) > 21)
            await HandleLossAsync("Bust! Dealer Win", 2);
    }

    [RelayCommand]
    public async Task StandAsync()
    {
        if (IsGameOver) return;
        IsGameActive = false;

        UpdateScores(true);
        OnFlipDealerCard?.Invoke();

        while (CalculateScore(DealerHand) < 17)
        {
            await Task.Delay(400); // เพิ่ม Delay นิดหน่อยให้ดูสมจริงขึ้น
            await DrawCardAsync(DealerHand, false, true); // ⚡ แก้แล้ว: isDealer = true
            UpdateScores(true);
        }

        await DetermineWinnerAsync();
    }

    private async Task DetermineWinnerAsync()
    {
        int p = CalculateScore(PlayerHand);
        int d = CalculateScore(DealerHand);

        if (d > 21) await HandleWinAsync("Dealer Bust! Your Win!", 1);
        else if (p > d) await HandleWinAsync("Your Win!", 1);
        else if (d > p) await HandleLossAsync("Dealer Win!", 2);
        else
        {
            EndGame("Push", Colors.Yellow, 0);
        }
    }

    private async Task HandleWinAsync(string msg, int winner)
    {
        if (_save == null) return;

        _lastRoundWinner = winner;
        DealerTaunt = "ไป... ชนะไปสิ!";

        EndGame(msg, Colors.Gold, winner);
    }

    private async Task HandleLossAsync(string msg, int winner)
    {
        if (_save == null) return;

        _lastRoundWinner = winner;
        // ⚡ Simple loss: just lose the bet (coins already deducted)
        DealerTaunt = "อ่าๆๆ ขอโทษนะ!";

        EndGame(msg, Colors.Red, winner);
    }

    private void EndGame(string msg, Color color, int winner)
    {
        IsGameOver = true;
        IsGameActive = false;
        IsBettingPhase = false; // ⚡ Hide betting UI when game ends
        IsRestartButtonVisible = true; // ⚡ Show restart button when game ends

        if (winner == 1) PlayerMatchScore++;
        else if (winner == 2) DealerMatchScore++;

        OnUpdateMatchDots?.Invoke(PlayerMatchScore, DealerMatchScore);

        if (PlayerMatchScore >= 3) { StatusText = "You're the champ!"; StatusColor = Colors.Gold; RestartButtonText = "Reset Match"; CompleteMatchAsync(true); }
        else if (DealerMatchScore >= 3) { StatusText = "แกแพ้แล้ว! ไอ้ขี้แพ้เอ้ย!ฮ่าๆ"; StatusColor = Colors.Red; RestartButtonText = "Reset Match"; CompleteMatchAsync(false); }
        else { StatusText = msg; StatusColor = color; RestartButtonText = "ตาถัดไป"; }
    }

    // ⚡ Call CasinoRigService only when match ends (someone gets 3 points)
    private async void CompleteMatchAsync(bool playerWon)
    {
        if (_save == null) return;

        if (playerWon)
        {
            var (result, coinDelta, dealerLine, debuff) = CasinoRigService.ResolveWin(CurrentBet);

            _save.Coins += coinDelta;
            if (result == CasinoWinResult.Cheated && debuff.HasValue)
            {
                CasinoRigService.ApplyDebuff(_save, debuff.Value);
            }

            Coins = _save.Coins;
            await _saveService.UpdateSaveAsync(_save);
            DealerTaunt = dealerLine;

            if (result == CasinoWinResult.Cheated)
            {
                if (OnShowBlackScreenDialog != null) await OnShowBlackScreenDialog(dealerLine);
            }
        }
        else
        {
            var (result, dealerLine) = CasinoRigService.ResolveLoss();

            if (result == CasinoLossResult.ItemStolen)
            {
                string stealMsg = CasinoRigService.StealItem(_save);
                dealerLine += $"\n(เสีย {stealMsg})";
            }

            await _saveService.UpdateSaveAsync(_save);
            DealerTaunt = dealerLine;

            if (result == CasinoLossResult.ItemStolen)
            {
                if (OnShowBlackScreenDialog != null) await OnShowBlackScreenDialog(dealerLine);
            }
        }
    }

    private void UpdateScores(bool showDealer)
    {
        PlayerScoreText = $"Your : {CalculateScore(PlayerHand)}";
        DealerScoreText = showDealer || DealerHand.Count < 2 ? $"Dealer : {CalculateScore(DealerHand)}" : $"Dealer : {DealerHand[1].Value} + ?";
    }

    private int CalculateScore(List<PlayingCard> hand)
    {
        int s = hand.Sum(c => c.Value);
        int aces = hand.Count(c => c.Rank == "A");
        while (s > 21 && aces > 0) { s -= 10; aces--; }
        return s;
    }
    // ⚡ คำสั่งสำหรับกดปุ่มย้อนกลับออกจากโต๊ะพนัน
    [RelayCommand]
    public async Task ExitGameAsync()
    {
        // สั่งให้ปิดหน้าต่างนี้ (PopModal) เพื่อกลับไปหน้าเมนูคาสิโน
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}