using System.Globalization;

namespace European.Converters
{
    public class PercentToProgressConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var number = value is double d ? d : (value is int i ? i : 0);
            return Math.Clamp(number / 100.0, 0, 1);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
