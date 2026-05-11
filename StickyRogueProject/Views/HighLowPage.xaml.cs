using StickyRogueProject.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace StickyRogueProject.Views;

public partial class HighLowPage : ContentPage
{
    private HighLowViewModel _viewModel;

    public HighLowPage(HighLowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.OnInitCards = async (current, next) => {
            stackCards.Children.Clear();
            stackCards.Children.Add(CreateCardUI(current, false));
            await AnimateCardIn(next, true);
        };

        _viewModel.OnShowNextCard = async (next) => {
            stackCards.Children[1] = CreateCardUI(next, false);
            await Task.CompletedTask;
        };

        _viewModel.OnProceedToNextRound = async (current, next) => {
            var oldCardUI = (Border)stackCards.Children[0];
            var newCardUI = (Border)stackCards.Children[1];
            await Task.WhenAll(
                oldCardUI.TranslateTo(-this.Width, 0, 500, Easing.CubicIn),
                oldCardUI.FadeTo(0, 500, Easing.Linear),
                newCardUI.TranslateTo(-130, 0, 500, Easing.CubicInOut)
            );
            stackCards.Children.Clear();
            stackCards.Children.Add(CreateCardUI(current, false));
            await AnimateCardIn(next, true);
        };

        // ⚡ เปิดหน้าจอดำเมื่อโดนโกง
        _viewModel.OnShowBlackScreenDialog = async (msg) => {
            await Navigation.PushModalAsync(new CasinoDialogPage(msg));
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
    protected override bool OnBackButtonPressed() => true;

    private async Task AnimateCardIn(PlayingCard card, bool isHidden)
    {
        Border cardUI = CreateCardUI(card, isHidden);
        cardUI.Opacity = 0;
        cardUI.TranslationX = this.Width > 0 ? this.Width : 1000;
        stackCards.Children.Add(cardUI);
        await Task.WhenAll(cardUI.TranslateTo(0, 0, 400, Easing.CubicOut), cardUI.FadeTo(1, 400, Easing.Linear));
    }

    private Border CreateCardUI(PlayingCard card, bool isHidden)
    {
        Border cardBorder = new Border
        {
            WidthRequest = 110,
            HeightRequest = 160,
            BackgroundColor = isHidden ? Colors.DarkRed : Colors.White,
            Stroke = Colors.Gold,
            StrokeThickness = 3,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }
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
}