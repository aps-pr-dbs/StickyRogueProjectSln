using System.Globalization;

namespace StickyRogueProject.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue)
            return isTrue ? 1.0 : 0.5; // ถ้ามี Save ปุ่มจะสว่าง (1.0), ถ้าไม่มีปุ่มจะจาง (0.5)
        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}