using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Seletor de mestrado com indicador de seleção (equivalente ao MasterProgramSelector).</summary>
    public class MasterProgramSelectorView : VerticalStackLayout
    {
        private readonly Action<string> _onSelect;

        public MasterProgramSelectorView(Action<string> onSelect)
        {
            _onSelect = onSelect;
            Spacing = 12;
        }

        public void SetPrograms(IEnumerable<MasterProgram> programs, string selectedId)
        {
            Children.Clear();
            foreach (var p in programs)
                Children.Add(CreateCard(p, p.Id == selectedId));
        }

        private Border CreateCard(MasterProgram program, bool selected)
        {
            var radio = new Border
            {
                StrokeThickness = 1.5,
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                Stroke = selected ? ThemeColors.Get("Primary") : ThemeColors.Get("Border"),
                WidthRequest = 22,
                HeightRequest = 22,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = selected ? ThemeColors.Get("Primary") : Colors.Transparent,
                Content = selected
                    ? new GlyphLabel
                    {
                        Glyph = Icons.Checkmark,
                        FontSize = 14,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                    }
                    : null,
            };

            var code = new Label
            {
                FontSize = 18,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
            };
            var name = new Label
            {
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };
            var textCol = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            textCol.Children.Add(code);
            textCol.Children.Add(name);

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                },
                ColumnSpacing = 12,
            };
            row.AddAt(radio, 0, 0);
            row.AddAt(textCol, 1, 0);

            var card = new Border
            {
                StrokeThickness = 1.5,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Stroke = selected ? ThemeColors.Get("Primary") : Colors.Transparent,
                BackgroundColor = selected ? ThemeColors.Get("PrimaryLight") : ThemeColors.Get("Surface"),
                Padding = new Thickness(16, 14),
                Content = row,
            };

            code.Text = program.Code;
            name.Text = program.FullName;

            var pressable = new PressableScale { Content = card, HorizontalOptions = LayoutOptions.Fill };
            pressable.Command = new RelayCommand<string>(_onSelect);
            pressable.CommandParameter = program.Id;
            return card;
        }
    }
}