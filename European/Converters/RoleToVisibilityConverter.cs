using System.Globalization;
using European.Enums;

namespace European.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Roles role || parameter is not string csv)
                return false;

            var allowed = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return allowed.Any(a => Enum.TryParse<Roles>(a, true, out var parsed) && parsed == role);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
