using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StickyRogueProject.ViewModels;

// คลาสเก็บข้อมูลไพ่
public class PlayingCard
{
    public string Suit { get; set; }
    public string Rank { get; set; }
    public int Value { get; set; }
    public Color CardColor { get; set; }
}

public partial class HighLowViewModel : ObservableObject
{
    private List<PlayingCard> _deck = new();
    public PlayingCard CurrentCard { get; private set; }
    public PlayingCard NextCard { get; private set; }

    [ObservableProperty] private int _streakCount;
    [ObservableProperty] private string _streakText = "Streak : 0 / 3";
    [ObservableProperty] private Color _streakColor = Colors.White;
    [ObservableProperty] private string _statusText = "What you think about next card?";
    [ObservableProperty] private Color _statusColor = Colors.LightGray;

    [ObservableProperty] private bool _isGameOver;
    [ObservableProperty] private bool _isGameActive = true;

    public bool IsAnimating { get; set; }

    // ⚡ ท่อส่งคำสั่งไปสั่งให้ View เล่นแอนิเมชัน
    public Action<PlayingCard, PlayingCard> OnInitCards { get; set; }
    public Func<PlayingCard, Task> OnShowNextCard { get; set; }
    public Func<PlayingCard, PlayingCard, Task> OnProceedToNextRound { get; set; }

    [RelayCommand]
    public void StartNewGame()
    {
        IsGameOver = false;
        IsGameActive = true;
        StreakCount = 0;
        UpdateStreakUI();

        StatusText = "What you think about next card?";
        StatusColor = Colors.LightGray;

        CreateDeck();
        CurrentCard = _deck[0]; _deck.RemoveAt(0);
        NextCard = _deck[0]; _deck.RemoveAt(0);

        // สั่งให้หน้าจอวาดไพ่เริ่มต้น
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
        if (IsGameOver || IsAnimating) return;
        IsAnimating = true;

        bool guessHigh = guess == "High";

        // 1. สั่งให้หน้าจอหงายไพ่
        if (OnShowNextCard != null) await OnShowNextCard(NextCard);

        await Task.Delay(800);

        // 2. เช็คผลลัพธ์
        bool isCorrect = (guessHigh && NextCard.Value > CurrentCard.Value) ||
                         (!guessHigh && NextCard.Value < CurrentCard.Value);

        if (isCorrect)
        {
            StreakCount++;
            UpdateStreakUI();

            if (StreakCount >= 3)
            {
                EndGame("3 Streak! You Win", Colors.Gold);
            }
            else
            {
                StatusText = "You did well! Guess again.";
                StatusColor = Colors.LightGreen;

                CurrentCard = NextCard;
                NextCard = _deck[0]; _deck.RemoveAt(0);

                // สั่งให้หน้าจอเลื่อนไพ่รอบต่อไป
                if (OnProceedToNextRound != null)
                    await OnProceedToNextRound(CurrentCard, NextCard);
            }
        }
        else
        {
            EndGame("Wrong! You lose all your streak", Colors.Red);
        }

        IsAnimating = false;
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
        StatusText = message;
        StatusColor = color;
    }
}