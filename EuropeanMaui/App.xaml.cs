using EuropeanMaui.Services;

namespace EuropeanMaui
{
    public partial class App : Application
    {
        private readonly ThemeSession _theme;
        private readonly NavigationService _navigation;

        public App(ThemeSession theme)
        {
            InitializeComponent();
            _theme = theme;
            _navigation = AppServices.Get<NavigationService>()!;

            AppServices.SetProvider(MauiProgram.Services);
            ApplyTheme(_theme.IsDark);
            _theme.ThemeChanged += () => ApplyTheme(_theme.IsDark);
        }

        /// <summary>Troca o dicionario de cores em tempo de execucao (claro/escuro).</summary>
        private void ApplyTheme(bool dark)
        {
            var merged = Resources.MergedDictionaries;
            var colors = merged
                .Where(d => d.Source?.OriginalString.EndsWith("Colors.xaml", StringComparison.OrdinalIgnoreCase) == true
                            || d.Source?.OriginalString.EndsWith("ColorsDark.xaml", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            foreach (var dictionary in colors)
                merged.Remove(dictionary);

            merged.Add(ThemePalettes.Get(dark));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell(_navigation, _theme);
            var window = new Window(shell) { Title = "AVA European & Icon Institute" };

            if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                bool hasSize = false;
                var display = DeviceDisplay.MainDisplayInfo;
                var size = new Size(
                    Math.Min(display.Width * 0.7, 1200),
                    Math.Min(display.Height * 0.8, 800));
                hasSize = display.Width > 0 && display.Height > 0;
                if (hasSize)
                {
                    window.Width = size.Width;
                    window.Height = size.Height;
                }
            }

            return window;
        }
    }
}