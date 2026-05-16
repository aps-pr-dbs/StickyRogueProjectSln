using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using StickyRogueProject.Models;

namespace StickyRogueProject.Views.PopUp;

public partial class RandomEventPopUpPage : Popup
{
    public string ResultAction { get; set; } = string.Empty;

    // ⚡ เพิ่มตัวล็อก
    private bool _isClosing = false;

    public RandomEventPopUpPage(GameEvent gameEvent)
    {
        InitializeComponent();
        BindingContext = gameEvent;
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        await CardContainer.ScaleTo(1, 500, Easing.SpringOut);
    }

    private async Task CloseWithAnimation(string action)
    {
        // ⚡ ถ้ากดปิดไปแล้ว ห้ามทำซ้ำ!
        if (_isClosing) return;
        _isClosing = true;

        ResultAction = action;
        await CardContainer.ScaleTo(0, 200, Easing.SpringIn);
        await CloseAsync();
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        await CloseWithAnimation("Normal");
    }

    private async void OnAcceptDealerClicked(object sender, EventArgs e)
    {
        // ป้องกันกรณีคลิกเบิ้ล
        if (_isClosing) return;
        _isClosing = true; // ล็อกปุ่มไว้ก่อนระหว่างที่ PopUp แจ้งเตือนเด้ง

        int outcome = new Random().Next(0, 2);
        if (outcome == 1)
        {
            var tcs = new TaskCompletionSource<bool>();
            await Navigation.PushModalAsync(new PopUp.GameMessagePopUpPage(
                "รอดตัวไป!",
                "ดีใจด้วย! คุณได้รับ ATK, DEF, INT +20 และ MAX HP, MAX MP +50",
                tcs
            ));
            await tcs.Task; // รอจนกว่าจะกด "ตกลง"

            _isClosing = false; // ⚡ ปลดล็อก! เพื่อให้ฟังก์ชัน CloseWithAnimation ยอมทำงาน
            await CloseWithAnimation("DealerWin");
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();
            await Navigation.PushModalAsync(new PopUp.GameMessagePopUpPage(
                "ดวงแตก...",
                "เสียใจด้วย, ค่า ATK, DEF, INT ของคุณลดลง -10 รวมไปถึง MAX HP, MAX MP ลดลง -20",
                tcs
            ));
            await tcs.Task; // รอจนกว่าจะกด "ตกลง"

            _isClosing = false; // ⚡ ปลดล็อก! เพื่อให้ฟังก์ชัน CloseWithAnimation ยอมทำงาน
            await CloseWithAnimation("DealerLose");
        }
    }

    private async void OnRunAwayClicked(object sender, EventArgs e)
    {
        // ⚡ ป้องกันกรณีกดเบิ้ล
        if (_isClosing) return;
        _isClosing = true; // ล็อกปุ่มชั่วคราว

        int escapeChance = new Random().Next(1, 5);
        if (escapeChance == 1)
        {
            RunAwayBtn.IsVisible = false;

            // ⚡ โดนจับได้ บังคับดื่มยา
            var tcs = new TaskCompletionSource<bool>();
            await Navigation.PushModalAsync(new PopUp.GameMessagePopUpPage(
                "โดนกระโดดจับตัว!",
                "เขาบล็อคไม่ให้คุณหนี! คุณหนีไม่สำเร็จ. คุณถูกบังคับให้ต้องดื่มยา",
                tcs
            ));
            await tcs.Task;

            _isClosing = false; // 💡 สำคัญมาก: หนีไม่สำเร็จ หน้าจอยังไม่ปิด ต้องปลดล็อกให้กดปุ่มอื่นต่อได้!
        }
        else
        {
            // ⚡ หนีรอดสำเร็จ
            var tcs = new TaskCompletionSource<bool>();
            await Navigation.PushModalAsync(new PopUp.GameMessagePopUpPage(
                "รอดตัวไป",
                "คุณหนีสำเร็จ!",
                tcs
            ));
            await tcs.Task;

            await CloseWithAnimation("Escaped");
        }
    }
}