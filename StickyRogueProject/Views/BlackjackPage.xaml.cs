using StickyRogueProject.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace StickyRogueProject.Views;

public partial class BlackjackPage : ContentPage
{
    private BlackjackViewModel _vm;

    public BlackjackPage(BlackjackViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = _vm;

        // เชื่อมแอนิเมชันจั่วไพ่ (โค้ดลื่นๆ ของเดิม)
        _vm.OnHitCard = async (hand, isHidden, isDealer) => {
            var card = hand.LastOrDefault();
            if (card == null) return;

            var uiStack = isDealer ? stackDealerCards : stackPlayerCards;
            Border cardUI = CreateCardUI(card, isHidden);
            cardUI.Opacity = 0;
            cardUI.TranslationX = this.Width > 0 ? this.Width : 1000;
            uiStack.Children.Add(cardUI);

            await Task.WhenAll(
                cardUI.TranslateTo(0, 0, 400, Easing.CubicOut),
                cardUI.FadeTo(1, 400, Easing.Linear)
            );
        };

        _vm.OnClearBoard = () => {
            stackPlayerCards.Children.Clear();
            stackDealerCards.Children.Clear();
        };

        _vm.OnUpdateMatchDots = (pScore, dScore) => {
            for (int i = 0; i < 3; i++)
            {
                ((Ellipse)stackPlayerMatchScore.Children[i]).Fill = (i < pScore) ? Colors.Gold : Colors.Gray;
                ((Ellipse)stackDealerMatchScore.Children[i]).Fill = (i < dScore) ? Colors.Gold : Colors.Gray;
            }
        };

        _vm.OnFlipDealerCard = () => {
            if (stackDealerCards.Children.Count > 0 && _vm.DealerHand.Count > 0)
                stackDealerCards.Children[0] = CreateCardUI(_vm.DealerHand[0], false);
        };

        // ⚡ เปิดหน้าจอดำเมื่อโดนโกง
        _vm.OnShowBlackScreenDialog = async (msg) => {
            await Navigation.PushModalAsync(new CasinoDialogPage(msg));
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeCommand.ExecuteAsync(null);
    }
    protected override bool OnBackButtonPressed() => true;

    private Border CreateCardUI(PlayingCard card, bool isHidden)
    {
        Border cardBorder = new Border
        {
            WidthRequest = 90,
            HeightRequest = 130,
            BackgroundColor = isHidden ? Colors.DarkBlue : Colors.White,
            Stroke = Colors.LightGray,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }
        };

        if (!isHidden)
        {
            Grid cardGrid = new Grid { Padding = 5 };
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start });
            cardGrid.Children.Add(new Label { Text = card.Suit, TextColor = card.CardColor, FontSize = 40, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
            cardGrid.Children.Add(new Label { Text = card.Rank + "\n" + card.Suit, TextColor = card.CardColor, FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Rotation = 180 });
            cardBorder.Content = cardGrid;
        }
        else
        {
            cardBorder.Content = new Label { Text = "STICK\nROGUE", TextColor = Colors.White, FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
        }
        return cardBorder;
    }
}