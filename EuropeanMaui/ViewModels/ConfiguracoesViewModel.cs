using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class ConfiguracoesViewModel : BaseViewModel
    {
        private readonly ThemeSession _theme;

        public ConfiguracoesViewModel(ThemeSession theme, LayoutService layout)
            : base(layout)
        {
            _theme = theme;
        }

        public ThemeSession Theme => _theme;
    }
}