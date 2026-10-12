using System.Globalization;

namespace European.Converters
{
    public class PercentageTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var number = value is double d ? d : 0;
            return $"{number:0}%";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
