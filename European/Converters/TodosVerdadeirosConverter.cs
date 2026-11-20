using System.Globalization;

namespace European.Converters
{
    /// <summary>MultiBinding: verdadeiro quando todos os valores são true.</summary>
    public class TodosVerdadeirosConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
            => values.All(v => v is true);

        public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
