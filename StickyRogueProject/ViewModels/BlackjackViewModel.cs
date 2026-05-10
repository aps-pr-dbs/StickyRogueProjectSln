using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace StickyRogueProject.ViewModels;

public partial class BlackjackViewModel : ObservableObject
{
    private List<PlayingCard> _deck = new();
    public List<PlayingCard> PlayerHand { get; private set; } = new();
    public List<PlayingCard> DealerHand { get; private set; } = new();

    [ObservableProperty] private int _playerMatchScore;
    [ObservableProperty] private int _dealerMatchScore;
    [ObservableProperty] private string _playerScoreText = "Your : 0";
    [ObservableProperty] private string _dealerScoreText = "Dealer : ?";
    [ObservableProperty] private string _statusText = "Your Turn";
    [ObservableProperty] private Color _statusColor = Colors.White;
    [ObservableProperty] private bool _isGameOver;
    [ObservableProperty] private bool _isGameActive = true;
    [ObservableProperty] private string _restartButtonText = "เริ่มเกมใหม่";

    // ⚡ Delegates สำหรับสั่ง View ให้เล่นแอนิเมชันจั่วไพ่
    public Func<List<PlayingCard>, bool, bool, Task> OnHitCard { get; set; }
    public Action OnClearBoard { get; set; }
    public Action<int, int> OnUpdateMatchDots { get; set; }
    public Action OnFlipDealerCard { get; set; }

    [RelayCommand]
    public async Task StartNewGameAsync()
    {
        if (PlayerMatchScore >= 3 || DealerMatchScore >= 3)
        {
            PlayerMatchScore = 0;
            DealerMatchScore = 0;
            OnUpdateMatchDots?.Invoke(0, 0);
        }

        IsGameOver = false;
        IsGameActive = true;
        StatusText = "Your Turn";
        StatusColor = Colors.White;

        PlayerHand.Clear();
        DealerHand.Clear();
        OnClearBoard?.Invoke();

        CreateDeck();

        if (OnHitCard != null)
        {
            await OnHitCard(PlayerHand, false, false);
            await OnHitCard(DealerHand, true, true); // ใบแรกบอทคว่ำ
            await OnHitCard(PlayerHand, false, false);
            await OnHitCard(DealerHand, false, false);
        }

        UpdateScores(false);
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
        if (IsGameOver || OnHitCard == null) return;

        await OnHitCard(PlayerHand, false, false);
        UpdateScores(false);

        if (CalculateScore(PlayerHand) > 21)
            EndGame("Bust! Dealer Win", Colors.Red, 2);
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
            await Task.Delay(200);
            await OnHitCard(DealerHand, false, false);
            UpdateScores(true);
        }

        DetermineWinner();
    }

    private void DetermineWinner()
    {
        int p = CalculateScore(PlayerHand);
        int d = CalculateScore(DealerHand);

        if (d > 21) EndGame("Dealer Bust! Your Win!", Colors.LightGreen, 1);
        else if (p > d) EndGame("Your Win!", Colors.LightGreen, 1);
        else if (d > p) EndGame("Dealer Win!", Colors.Red, 2);
        else EndGame("Push", Colors.Yellow, 0);
    }

    private void EndGame(string msg, Color color, int winner)
    {
        IsGameOver = true;
        IsGameActive = false;

        if (winner == 1) PlayerMatchScore++;
        else if (winner == 2) DealerMatchScore++;

        OnUpdateMatchDots?.Invoke(PlayerMatchScore, DealerMatchScore);

        if (PlayerMatchScore >= 3) { StatusText = "Your the champ!"; StatusColor = Colors.Gold; RestartButtonText = "Reset"; }
        else if (DealerMatchScore >= 3) { StatusText = "You lose"; StatusColor = Colors.Red; RestartButtonText = "Reset"; }
        else { StatusText = msg; StatusColor = color; RestartButtonText = "Next round"; }
    }

    private void UpdateScores(bool showDealer)
    {
        PlayerScoreText = $"Your : {CalculateScore(PlayerHand)}";
        DealerScoreText = showDealer ? $"Dealer : {CalculateScore(DealerHand)}" : $"Dealer : {DealerHand[1].Value} + ?";
    }

    private int CalculateScore(List<PlayingCard> hand)
    {
        int s = hand.Sum(c => c.Value);
        int aces = hand.Count(c => c.Rank == "A");
        while (s > 21 && aces > 0) { s -= 10; aces--; }
        return s;
    }
}