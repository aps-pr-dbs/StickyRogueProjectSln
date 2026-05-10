using System.Globalization;

namespace StickyRogueProject.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue)
            return !isTrue; // สลับ True เป็น False (เพื่อเอาไปใช้ซ่อน/แสดง ข้อความเตือน)
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}