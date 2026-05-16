using System.Threading.Tasks;

namespace StickyRogueProject.Views.PopUp;

public partial class InventoryActionPopUpPage : ContentPage
{
    private readonly TaskCompletionSource<string> _tcs;
    private bool _isClosing = false;

    public InventoryActionPopUpPage(string itemName, bool isEquipped, TaskCompletionSource<string> tcs)
    {
        InitializeComponent();
        _tcs = tcs;
        ItemNameLabel.Text = itemName;

        // ถ้าใส่แก้อยู่ ให้เปลี่ยนปุ่มเป็น "ถอดออก"
        if (isEquipped)
        {
            PrimaryBtn.Text = "ถอดออก (Unequip)";
            PrimaryBtn.BackgroundColor = Color.FromArgb("#2E3B4D"); // สีน้ำเงินดาร์กๆ
            PrimaryBtn.BorderColor = Color.FromArgb("#98C1FB");
            PrimaryBtn.TextColor = Color.FromArgb("#98C1FB");
        }
    }

    private async void OnPrimaryClicked(object sender, EventArgs e)
    {
        await CloseWithResult(PrimaryBtn.Text.Contains("สวมใส่") ? "Equip" : "Unequip");
    }

    private async void OnDiscardClicked(object sender, EventArgs e)
    {
        await CloseWithResult("Discard");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseWithResult("Cancel");
    }

    private async Task CloseWithResult(string result)
    {
        if (_isClosing) return;
        _isClosing = true;

        await Navigation.PopModalAsync();
        _tcs.TrySetResult(result);
    }

    protected override bool OnBackButtonPressed() => true; // ล็อกปุ่ม Back
}