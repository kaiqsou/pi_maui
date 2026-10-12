using System.Globalization;
using European.Enums;

namespace European.Converters
{
    public class StatusMatriculaToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not StatusMatricula status)
                return Colors.Gray;

            var key = status switch
            {
                StatusMatricula.Ativa => "BrandPrimary",
                StatusMatricula.Concluida => "Success",
                StatusMatricula.Pendente => "Warning",
                StatusMatricula.Trancada => "OnSurfaceVariant",
                StatusMatricula.Cancelada => "Danger",
                _ => "OnSurfaceVariant"
            };

            return Application.Current?.Resources.TryGetValue(key, out var color) == true && color is Color c
                ? c
                : Colors.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
