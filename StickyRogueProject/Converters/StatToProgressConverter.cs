// StatToProgressConverter แปลงค่า Stat (int) ให้เป็นเลขทศนิยม 0.0 - 1.0
// เพื่อใช้กับ ProgressBar ใน ClassSelectPage.xaml
// ตัวอย่าง:
//   StatAtk = 14, MaxStat = 20 → 14 / 20 = 0.70
//   ProgressBar จะแสดงแถบยาว 70% ของความกว้างทั้งหมด

using System.Globalization;

namespace StickyRogueProject.Converters;

public class StatToProgressConverter : IValueConverter
{
    // Convert — เรียกโดย XAML เมื่อข้อมูลไหลจาก ViewModel → View
    // value     = ค่า Stat จริง เช่น 14
    // parameter = ค่า Stat สูงสุดที่เป็นไปได้ เช่น "20"
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // ตรวจสอบว่า value และ parameter ถูกส่งมาครบ
        if (value is int statValue && parameter is string maxString
            && int.TryParse(maxString, out int maxStat) && maxStat > 0)
        {
            // หารค่า Stat ด้วยค่า Max เพื่อให้ได้สัดส่วน 0.0 - 1.0
            // Math.Clamp ป้องกันไม่ให้ค่าเกิน 1.0 หรือต่ำกว่า 0.0
            return Math.Clamp((double)statValue / maxStat, 0.0, 1.0);
        }

        // ถ้าข้อมูลไม่ถูกต้อง คืนค่า 0 (Progress Bar ว่างเปล่า)
        return 0.0;
    }

    // ConvertBack — ไม่จำเป็นต้องใช้งาน เพราะ ProgressBar เป็น One-Way Binding
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
