using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Card de resumo do mestrado (equivalente ao ProgramCard do front-end).</summary>
    public class ProgramCardView : Border
    {
        private readonly Label _statsLabel;
        private readonly Label _percentLabel;
        private readonly ProgressBarView _progress;

        public ProgramCardView()
        {
            StrokeThickness = 1;
            Stroke = Colors.Transparent;
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = ThemeColors.Get("Surface");
            Padding = new Thickness(20);

            _percentLabel = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Accent"),
                HorizontalTextAlignment = TextAlignment.End,
            };

            var iconBox = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = ThemeColors.Get("Accent"),
                WidthRequest = 44,
                HeightRequest = 44,
                Content = new GlyphLabel
                {
                    Glyph = Icons.School,
                    FontSize = 22,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };

            var header = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            var headCol = new VerticalStackLayout { Spacing = 2 };
            var overline = new Label
            {
                Text = AppText.Dashboard.CurrentProgram.ToUpperInvariant(),
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Accent"),
                CharacterSpacing = 0.6,
            };
            var codeLabel = new Label
            {
                FontSize = 24,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
            };
            var fullNameLabel = new Label
            {
                FontSize = 14,
                TextColor = ThemeColors.Get("Text"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };
            headCol.Children.Add(overline);
            headCol.Children.Add(codeLabel);
            headCol.Children.Add(fullNameLabel);
            header.AddAt(headCol, 0, 0);
            header.AddAt(iconBox, 1, 0);

            var subtitleLabel = new Label
            {
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2,
                IsVisible = false,
            };

            _statsLabel = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("TextMuted"),
            };

            var statsRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };
            statsRow.AddAt(_statsLabel, 0, 0);
            statsRow.AddAt(_percentLabel, 1, 0);

            _progress = new ProgressBarView
            {
                FillColorKey = "Accent",
                TrackColor = Color.FromArgb("#26FFFFFF"),
            };

            var stack = new VerticalStackLayout { Spacing = 20 };
            stack.Children.Add(header);
            stack.Children.Add(subtitleLabel);
            stack.Children.Add(statsRow);
            stack.Children.Add(_progress);

            Content = stack;

            Code = codeLabel;
            FullName = fullNameLabel;
            Subtitle = subtitleLabel;
        }

        public Label Code { get; }
        public Label FullName { get; }
        public Label Subtitle { get; }

        private MasterProgram? _program;

        public MasterProgram? Program
        {
            set
            {
                _program = value;
                if (value == null) return;
                Code.Text = value.Code;
                FullName.Text = value.FullName;
                Subtitle.Text = value.Subtitle;
                Subtitle.IsVisible = !string.IsNullOrEmpty(value.Subtitle);
                _statsLabel.Text = $"{value.Modules.Count} {AppText.Modulos.Title.ToLowerInvariant()} · {value.TotalHours}{AppText.Modulos.Hours}";
                _percentLabel.Text = $"{value.AverageProgress}% {AppText.Modulos.CompletedLabel}";
                _progress.Progress = value.AverageProgress;
            }
        }
    }
}