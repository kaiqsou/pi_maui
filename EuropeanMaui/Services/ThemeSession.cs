using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Estado global do tema claro/escuro (equivalente ao useThemeStore, persistido).
    /// </summary>
    public class ThemeSession : INotifyPropertyChanged
    {
        private const string KEY = "ava-theme";
        private bool _isDark = Preferences.Default.Get(KEY, false);

        public static ThemeSession? Instance { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDark
        {
            get => _isDark;
            private set => SetField(ref _isDark, value);
        }

        public ThemeSession()
        {
            Preferences.Default.Set(KEY, IsDark);
            Instance = this;
        }

        public void Toggle()
        {
            IsDark = !IsDark;
            Preferences.Default.Set(KEY, IsDark);
            ThemeChanged?.Invoke();
        }

        public event Action? ThemeChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}