using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Detecta a largura da janela e expõe breakpoints (equivalente ao useBreakpoints).
    /// </summary>
    public class LayoutService : INotifyPropertyChanged
    {
        public const double MobileBreakpoint = 768;
        public const double DesktopBreakpoint = 1024;

        public event PropertyChangedEventHandler? PropertyChanged;

        private double _width = 400;
        private double _height = 750;

        public double Width
        {
            get => _width;
            private set => SetField(ref _width, value);
        }

        public double Height
        {
            get => _height;
            private set => SetField(ref _height, value);
        }

        public bool IsMobile => Width < MobileBreakpoint;
        public bool IsTablet => Width >= MobileBreakpoint && Width < DesktopBreakpoint;
        public bool IsDesktop => Width >= DesktopBreakpoint;

        public void Update(double width, double height)
        {
            Width = width;
            Height = height;
            OnPropertyChanged(nameof(IsMobile));
            OnPropertyChanged(nameof(IsTablet));
            OnPropertyChanged(nameof(IsDesktop));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}