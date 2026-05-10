using Microsoft.Maui.Controls.Shapes;

namespace StickyRogueProject.Views;

public partial class HighLowPage : ContentPage
{
    public class PlayingCard
    {
        public string Suit { get; set; }
        public string Rank { get; set; }
        public int Value { get; set; }
        public Color CardColor { get; set; }
    }

    List<PlayingCard> deck = new List<PlayingCard>();
    PlayingCard currentCard; // ไพ่ใบหลัก (หงาย)
    PlayingCard nextCard;    // ไพ่ใบที่ต้องทาย (คว่ำ)

    int streakCount = 0;
    bool isGameOver = false;
    bool isAnimating = false; // ป้องกันคนกดปุ่มรัวๆ ตอนไพ่กำลังสไลด์

    public HighLowPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        StartNewGame();
    }

    // ล็อกปุ่ม Back ของเครื่อง
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void StartNewGame()
    {
        isGameOver = false;
        streakCount = 0;
        UpdateStreakUI();

        btnHigh.IsVisible = true;
        btnLow.IsVisible = true;
        btnNewGame.IsVisible = false;

        lblStatus.Text = "What you think about next card?";
        lblStatus.TextColor = Colors.LightGray;

        CreateDeck();
        await DrawInitialCards();
    }

    private void CreateDeck()
    {
        deck.Clear();
        string[] suits = { "♠", "♥", "♦", "♣" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        foreach (var suit in suits)
        {
            Color color = (suit == "♥" || suit == "♦") ? Colors.Red : Colors.Black;
            foreach (var rank in ranks)
            {
                int value = 0;
                // ในเกม High/Low ไพ่ A มักจะมีค่าสูงสุดคือ 14
                if (rank == "J") value = 11;
                else if (rank == "Q") value = 12;
                else if (rank == "K") value = 13;
                else if (rank == "A") value = 14;
                else value = int.Parse(rank);

                deck.Add(new PlayingCard { Suit = suit, Rank = rank, Value = value, CardColor = color });
            }
        }
        // สับไพ่
        deck = deck.OrderBy(x => Random.Shared.Next()).ToList();
    }

    private async Task DrawInitialCards()
    {
        stackCards.Children.Clear();

        // จั่ว 2 ใบ
        currentCard = deck[0]; deck.RemoveAt(0);
        nextCard = deck[0]; deck.RemoveAt(0);

        // ใบซ้าย: หงายไพ่ทันที
        stackCards.Children.Add(CreateCardUI(currentCard, false));

        // ใบขวา: คว่ำไพ่ แล้วเล่นแอนิเมชันสไลด์
        await AnimateCardIn(nextCard, true);
    }

    private async Task AnimateCardIn(PlayingCard card, bool isHidden)
    {
        isAnimating = true;
        Border cardUI = CreateCardUI(card, isHidden);
        cardUI.Opacity = 0;

        double startingX = this.Width > 0 ? this.Width : 1000;
        cardUI.TranslationX = startingX;

        stackCards.Children.Add(cardUI);

        // จั่วใบใหม่สไลด์เข้ามา
        await Task.WhenAll(
            cardUI.TranslateTo(0, 0, 400, Easing.CubicOut),
            cardUI.FadeTo(1, 400, Easing.Linear)
        );
        isAnimating = false;
    }

    private Border CreateCardUI(PlayingCard card, bool isHidden)
    {
        Border cardBorder = new Border
        {
            WidthRequest = 110,  // ปรับให้การ์ดใหญ่ขึ้นนิดนึงสำหรับเกมนี้
            HeightRequest = 160,
            BackgroundColor = isHidden ? Colors.DarkRed : Colors.White,
            Stroke = Colors.Gold,
            StrokeThickness = 3, // ขอบสีทองหนาๆ ดูพรีเมียม
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Margin = new Thickness(0, 0, 0, 0)
        };

        if (!isHidden)
        {
            Grid cardGrid = new Grid { Padding = 5 };
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start });
            cardGrid.Children.Add(new Label { Text = card.Suit, TextColor = card.CardColor, FontSize = 50, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Rotation = 180 });
            cardBorder.Content = cardGrid;
        }
        else
        {
            cardBorder.Content = new Label { Text = "❓", TextColor = Colors.White, FontSize = 50, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
        return cardBorder;
    }

    private async void BtnHigh_Clicked(object sender, EventArgs e)
    {
        await ProcessGuess(true);
    }

    private async void BtnLow_Clicked(object sender, EventArgs e)
    {
        await ProcessGuess(false);
    }

    private async Task ProcessGuess(bool guessHigh)
    {
        if (isGameOver || isAnimating) return;

        // 1. หงายไพ่ใบที่สอง
        stackCards.Children[1] = CreateCardUI(nextCard, false);
        // หน่วงเวลาให้ผู้เล่นลุ้นและดูผล 800ms (สั้นลงเพราะมีแอนิเมชัน)
        await Task.Delay(800);

        // 2. เช็คว่าทายถูกไหม
        bool isCorrect = false;
        if (guessHigh && nextCard.Value > currentCard.Value) isCorrect = true;
        if (!guessHigh && nextCard.Value < currentCard.Value) isCorrect = true;

        if (isCorrect)
        {
            streakCount++;
            UpdateStreakUI();

            if (streakCount >= 3)
            {
                EndGame("3 Streak! You Win", Colors.Gold);
            }
            else
            {
                lblStatus.Text = "You did well! Guess again.";
                lblStatus.TextColor = Colors.LightGreen;
                // NEW: ใช้ฟังก์ชันแอนิเมชันสไลด์
                await ProceedToNextRoundWithAnimation();
            }
        }
        else
        {
            // ทายผิด
            EndGame("Wrong! You lose all your streak", Colors.Red);
        }
    }

    private async Task ProceedToNextRoundWithAnimation()
    {
        isAnimating = true;

        // ดึง Border ของไพ่ทั้ง 2 ใบออกมา
        var oldCardUI = (Border)stackCards.Children[0]; // ใบซ้าย (ใบหลักเก่า)
        var newCardUI = (Border)stackCards.Children[1]; // ใบขวา (ใบที่เพิ่งทาย)

        uint animDuration = 500; // เวลาแอนิเมชัน (0.5 วินาที)

        // 1. เล่นแอนิเมชันเลื่อนพร้อมๆ กัน
        await Task.WhenAll(
            // ใบซ้าย: สไลด์ตกจอไปทางซ้ายสุดๆ (-Width) และจางหายไป
            oldCardUI.TranslateTo(-this.Width, 0, animDuration, Easing.CubicIn),
            oldCardUI.FadeTo(0, animDuration, Easing.Linear),

            // ใบขวา: สไลด์มาทางซ้าย เพื่อมาแทนที่ตำแหน่งตรงกลาง
            // (-130 คือขยับไปทางซ้ายประมาณขนาดการ์ด + spacing)
            newCardUI.TranslateTo(-130, 0, animDuration, Easing.CubicInOut)
        );

        // 2. แอนิเมชันเสร็จสิ้น จัดการข้อมูลใหม่
        isAnimating = false; // ปลดล็อก

        // ก้าวไปสู่รอบถัดไป
        currentCard = nextCard;
        nextCard = deck[0]; deck.RemoveAt(0);

        // 3. จัดการหน้าจอใหม่ ให้สะอาด
        stackCards.Children.Clear();

        // 4. เอาไพ่หลักไปวางทางซ้าย (แบบไม่ต้องสไลด์แล้ว)
        stackCards.Children.Add(CreateCardUI(currentCard, false));

        // 5. สไลด์ไพ่ใบใหม่ (คว่ำ) มาทางขวา เหมือนเดิม
        await AnimateCardIn(nextCard, true);
    }

    private void UpdateStreakUI()
    {
        lblStreak.Text = $"Streak : {streakCount} / 3";
        lblStreak.TextColor = (streakCount == 0) ? Colors.White : Colors.LightGreen;
    }

    private void EndGame(string message, Color color)
    {
        isGameOver = true;
        lblStatus.Text = message;
        lblStatus.TextColor = color;

        btnHigh.IsVisible = false;
        btnLow.IsVisible = false;
        btnNewGame.IsVisible = true;
    }

    private void BtnNewGame_Clicked(object sender, EventArgs e)
    {
        StartNewGame();
    }

    //private async void BtnExitToMenu_Clicked(object sender, EventArgs e)
    //{
    //    await Navigation.PopAsync(); // กลับไปหน้าเมนูหลัก
    //}
}