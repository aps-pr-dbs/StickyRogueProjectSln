using Microsoft.Maui.Controls.Shapes;

namespace StickyRogueProject.Views;

public partial class BlackjackPage : ContentPage
{
    bool isGameOver = false; // ของเดิม
    int playerMatchScore = 0; // เก็บแต้มใหญ่ผู้เล่น
    int dealerMatchScore = 0; // เก็บแต้มใหญ่บอท
    // คลาสไพ่แบบง่ายๆ ซ้อนไว้ข้างใน
    public class PlayingCard
    {
        public string Suit { get; set; } // ดอกไพ่ (♠, ♥, ♦, ♣)
        public string Rank { get; set; } // เลขไพ่ (2-10, J, Q, K, A)
        public int Value { get; set; }   // ค่าแต้ม
        public Color CardColor { get; set; } // สีแดงหรือดำ
    }

    List<PlayingCard> deck = new List<PlayingCard>();
    List<PlayingCard> playerHand = new List<PlayingCard>();
    List<PlayingCard> dealerHand = new List<PlayingCard>();

    public BlackjackPage()
    {
        InitializeComponent();

    }
    protected override bool OnBackButtonPressed()
    {
        // การ return true; แปลว่า "แอปพลิเคชันขอจัดการปุ่ม Back เอง ระบบมือถือไม่ต้องยุ่ง"
        // (พูดง่ายๆ คือกดยังไงก็ไม่เกิดอะไรขึ้นครับ ล็อกตายไว้เลย)
        return true;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // รอสักนิดเพื่อให้ Layout ทำงานเสร็จสมบูรณ์
        await Task.Delay(100);
        StartNewGame();
    }

    private async void StartNewGame() // เปลี่ยนเป็น async เพื่อใช้ await ข้างใน
    {
        isGameOver = false; // ของเดิม
        btnHit.IsVisible = true;
        btnStand.IsVisible = true;
        btnNewGame.IsVisible = false;
        lblStatus.Text = "Your Turn";
        lblStatus.TextColor = Colors.White;

        playerHand.Clear();
        dealerHand.Clear();
        stackPlayerCards.Children.Clear();
        stackDealerCards.Children.Clear();

        CreateDeck();

        // แจกไพ่เริ่มต้น ฝั่งละ 2 ใบ
        // เราใช้ await เพื่อให้ไพ่สไลด์มาทีละใบ เหมือนคนแจกจริงๆ
        await HitCard(playerHand, stackPlayerCards, false);
        await HitCard(dealerHand, stackDealerCards, true); // ไพ่ใบแรกบอทคว่ำไว้
        await HitCard(playerHand, stackPlayerCards, false);
        await HitCard(dealerHand, stackDealerCards, false);

        UpdateScores(false);
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
                if (rank == "J" || rank == "Q" || rank == "K") value = 10;
                else if (rank == "A") value = 11; // ให้ Ace เป็น 11 ไว้ก่อน ค่อยไปหักลบทีหลัง
                else value = int.Parse(rank);

                deck.Add(new PlayingCard { Suit = suit, Rank = rank, Value = value, CardColor = color });
            }
        }

        // สับไพ่
        deck = deck.OrderBy(x => Random.Shared.Next()).ToList();
    }

    private async Task HitCard(List<PlayingCard> hand, HorizontalStackLayout uiStack, bool isHidden)
    {
        if (deck.Count == 0) return;

        var card = deck[0];
        deck.RemoveAt(0);
        hand.Add(card);

        // 1. วาดไพ่ลง UI (แต่ซ่อนไว้ก่อน)
        Border cardUI = CreateCardUI(card, isHidden);
        cardUI.Opacity = 1; // ตั้งค่าเริ่มต้นให้จางหายไป

        double startingX = this.Width > 0 ? this.Width : 1000;
        cardUI.TranslationX = startingX;

        cardUI.Scale = 1; // ตั้งค่าเริ่มต้นให้มีขนาดเล็ก 0.8

        uiStack.Children.Add(cardUI);

        uint animDuration = 400;

        // 2. เริ่มเล่นแอนิเมชันพร้อมๆ กัน
        // เคลื่อนที่กลับมาที่ตำแหน่งเดิม (0,0), ขยายขนาดเป็นปกติ (1), และค่อยๆ ปรากฏขึ้น (1)
        await Task.WhenAll(
            // สไลด์กลับมาที่ตำแหน่งเดิม (0)
            cardUI.TranslateTo(0, 0, animDuration, Easing.CubicOut),
            cardUI.ScaleTo(1, animDuration, Easing.CubicOut),
            cardUI.FadeTo(1, animDuration, Easing.Linear) // Linear Fade จะดูนุ่มนวลกว่าพร้อม Slide ยาว
        );
    }

    private Border CreateCardUI(PlayingCard card, bool isHidden)
    {
        Border cardBorder = new Border
        {
            WidthRequest = 90,
            HeightRequest = 130,
            BackgroundColor = isHidden ? Colors.DarkBlue : Colors.White,
            Stroke = Colors.LightGray,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Margin = new Thickness(0, 0, 0, 0),
           
        };

        if (!isHidden)
        {
            Grid cardGrid = new Grid { Padding = 5 };
            // ตัวเลขมุมบนซ้าย
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start });
            // สัญลักษณ์ตรงกลางใหญ่ๆ
            cardGrid.Children.Add(new Label { Text = card.Suit, TextColor = card.CardColor, FontSize = 40, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
            // ตัวเลขมุมล่างขวา (กลับหัวนิดนึงให้ดูเรียล)
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Rotation = 180 });

            cardBorder.Content = cardGrid;
        }
        else
        {
            // ลายหลังไพ่
            cardBorder.Content = new Label { Text = "STICK\nROUGE", TextColor = Colors.White, FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
        }

        return cardBorder;
    }

    private int CalculateScore(List<PlayingCard> hand)
    {
        int score = 0;
        int acesCount = 0;

        foreach (var card in hand)
        {
            score += card.Value;
            if (card.Rank == "A") acesCount++;
        }

        // ลอจิกปรับค่า Ace: ถ้าแต้มเกิน 21 และมีไพ่ Ace ให้เปลี่ยนค่า Ace จาก 11 เป็น 1 (หักออก 10 แต้ม)
        while (score > 21 && acesCount > 0)
        {
            score -= 10;
            acesCount--;
        }

        return score;
    }

    private void UpdateScores(bool showDealerFullScore)
    {
        int playerScore = CalculateScore(playerHand);
        lblPlayerScore.Text = $"Your : {playerScore}";

        if (showDealerFullScore)
        {
            lblDealerScore.Text = $"Dealer : {CalculateScore(dealerHand)}";
            // หงายไพ่ใบแรกของบอท
            stackDealerCards.Children[0] = CreateCardUI(dealerHand[0], false);
        }
        else
        {
            // โชว์แค่แต้มไพ่ใบที่สองของบอท
            lblDealerScore.Text = $"Dealer : {dealerHand[1].Value} + ?";
        }
    }

    private async void BtnHit_Clicked(object sender, EventArgs e)
    {
        if (isGameOver) return;

        // ต้องใส่ await เพื่อรอให้แอนิเมชันจั่วไพ่เสร็จ
        await HitCard(playerHand, stackPlayerCards, false);
        UpdateScores(false);

        int playerScore = CalculateScore(playerHand);
        if (playerScore > 21)
        {
            EndGame("Bust! Dealer Win", Colors.Red ,2);
        }
    }

    private async void BtnStand_Clicked(object sender, EventArgs e)
    {
        if (isGameOver) return;

        btnHit.IsVisible = false;
        btnStand.IsVisible = false;
        UpdateScores(true); // หงายไพ่บอท

        // ลอจิกบอท: แต้ม <= 16 ต้องจั่ว, >= 17 ต้องหยุด
        while (CalculateScore(dealerHand) < 17)
        {
            await Task.Delay(200); // หน่วงเวลาเล็กน้อยก่อนบอทเริ่มจั่ว (สั้นลงเพราะมีแอนิเมชันแล้ว)
            // ต้องใส่ await ตรงนี้
            await HitCard(dealerHand, stackDealerCards, false);
            UpdateScores(true);
        }

        DetermineWinner();
    }

    private void UpdateMatchScoreUI()
    {
        // อัปเดตสีจุดของผู้เล่น
        for (int i = 0; i < 3; i++)
        {
            var dot = (Ellipse)stackPlayerMatchScore.Children[i];
            dot.Fill = (i < playerMatchScore) ? Colors.Gold : Colors.Gray;
        }

        // อัปเดตสีจุดของบอท
        for (int i = 0; i < 3; i++)
        {
            var dot = (Ellipse)stackDealerMatchScore.Children[i];
            dot.Fill = (i < dealerMatchScore) ? Colors.Gold : Colors.Gray;
        }
    }

    private void DetermineWinner()
    {
        int playerScore = CalculateScore(playerHand);
        int dealerScore = CalculateScore(dealerHand);

        if (dealerScore > 21)
        {
            EndGame("Dealer Bust! Your Win!", Colors.LightGreen, 1); // 1 = ผู้เล่นชนะ
        }
        else if (playerScore > dealerScore)
        {
            EndGame("Your Win!", Colors.LightGreen, 1);
        }
        else if (dealerScore > playerScore)
        {
            EndGame("Dealer Win!", Colors.Red, 2); // 2 = บอทชนะ
        }
        else
        {
            EndGame("Push", Colors.Yellow, 0); // 0 = เสมอ ไม่มีใครได้แต้ม
        }
    }

    private void EndGame(string message, Color color, int winner)
    {

        // บวกแต้มตามผู้ชนะ
        if (winner == 1) playerMatchScore++;
        else if (winner == 2) dealerMatchScore++;

        UpdateMatchScoreUI(); // ระบายสีจุดใหม่

        // เช็คว่ามีใครได้ 3 แต้มหรือยัง?
        if (playerMatchScore >= 3)
        {
            lblStatus.Text = "Your the champ!";
            lblStatus.TextColor = Colors.Gold;
            btnNewGame.Text = "Reset";
        }
        else if (dealerMatchScore >= 3)
        {
            lblStatus.Text = "You lose";
            lblStatus.TextColor = Colors.Red;
            btnNewGame.Text = "Reset";
        }
        else
        {
            // ยังไม่มีใครครบ 3 แต้ม
            lblStatus.Text = message;
            lblStatus.TextColor = color;
            btnNewGame.Text = "Next round";
        }

        btnHit.IsVisible = false;
        btnStand.IsVisible = false;
        btnNewGame.IsVisible = true;
        UpdateScores(true);
    }

    private void BtnNewGame_Clicked(object sender, EventArgs e)
    {
        // ถ้ามีคนครบ 3 แต้ม ให้รีเซ็ตแต้มกลับเป็น 0 ใหม่หมด
        if (playerMatchScore >= 3 || dealerMatchScore >= 3)
        {
            playerMatchScore = 0;
            dealerMatchScore = 0;
            UpdateMatchScoreUI(); // คืนสีเทาให้จุด
        }

        StartNewGame();
    }
}