using System.Collections;
using System.Globalization;

namespace European.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var count = value switch
            {
                int i => i,
                ICollection c => c.Count,
                IEnumerable e => e.Cast<object>().Count(),
                _ => 0
            };

            var hasItems = count > 0;
            return parameter as string == "invert" ? !hasItems : hasItems;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
