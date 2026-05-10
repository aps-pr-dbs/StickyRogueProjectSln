using StickyRogueProject.Models; 

namespace StickyRogueProject.Views.PopUp; 

public partial class CharacterStatusPage : ContentPage
{
    // เปลี่ยนจาก Character เป็น ActiveSave เพื่อให้รับข้อมูลจาก Save ของเราได้
    public CharacterStatusPage(ActiveSave save)
    {
        InitializeComponent();

        
        BindingContext = save;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        
        await Navigation.PopModalAsync();
    }
}