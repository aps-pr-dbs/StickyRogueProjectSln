using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class CharacterStatusPage : ContentPage
{
    public CharacterStatusPage(ActiveSave save)
    {
        InitializeComponent();

        BindingContext = save;

        // ⚡ คำนวณความยาวหลอด ProgressBar (ค่าต้องอยู่ระหว่าง 0.0 ถึง 1.0)
        if (save.MaxHp > 0)
        {
            barHp.Progress = (double)save.CurrentHp / save.MaxHp;
        }

        if (save.MaxMp > 0)
        {
            barMp.Progress = (double)save.CurrentMp / save.MaxMp;
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}