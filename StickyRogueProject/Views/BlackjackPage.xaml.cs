using StickyRogueProject.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace StickyRogueProject.Views;

public partial class BlackjackPage : ContentPage
{
    private BlackjackViewModel _vm;

    public BlackjackPage()
    {
        InitializeComponent();
        _vm = new BlackjackViewModel();
        BindingContext = _vm;

        // ⚡ เชื่อมโยงแอนิเมชัน
        _vm.OnHitCard = async (hand, isHidden, isDealer) => {

            var card = hand.LastOrDefault();
            if (card == null) return;

            await Task.Delay(400);
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        _vm.StartNewGameCommand.Execute(null);
    }
    protected override bool OnBackButtonPressed() => true;
}