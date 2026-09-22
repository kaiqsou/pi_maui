using System.Windows.Input;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Card "Continue de onde parou" (equivalente ao ContinueCard do front-end).</summary>
    public class ContinueCardView : Border
    {
        private readonly Label _titleLabel;
        private readonly Label _subtitleLabel;
        private readonly Label _pillLabel;
        private readonly ProgressBarView _progress;
        private readonly ButtonView _continueButton;

        public ContinueCardView()
        {
            StrokeThickness = 1;
            Stroke = ThemeColors.Get("Border");
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = ThemeColors.Get("Surface");
            Padding = new Thickness(16);

            _progress = new ProgressBarView { FillColorKey = "Accent", HeightRequest = 8 };

            var iconBox = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = ThemeColors.Get("SurfaceAlt"),
                WidthRequest = 40,
                HeightRequest = 40,
                Content = new GlyphLabel
                {
                    Glyph = Icons.Book,
                    FontSize = 20,
                    TextColor = ThemeColors.Get("Accent"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };

            var caption = new Label
            {
                Text = AppText.Dashboard.ContinueWhere,
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Accent"),
            };
            _titleLabel = new Label
            {
                FontSize = 14,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };
            _subtitleLabel = new Label
            {
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                IsVisible = false,
            };

            var textCol = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            textCol.Children.Add(caption);
            textCol.Children.Add(_titleLabel);
            textCol.Children.Add(_subtitleLabel);

            _pillLabel = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("PrimaryDark"),
                HorizontalTextAlignment = TextAlignment.Center,
            };
            var pill = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                BackgroundColor = ThemeColors.Get("PrimaryLight"),
                Padding = new Thickness(12, 4),
                VerticalOptions = LayoutOptions.Center,
                Content = _pillLabel,
            };

            var topRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            topRow.AddAt(iconBox, 0, 0);
            topRow.AddAt(textCol, 1, 0);
            topRow.AddAt(pill, 2, 0);

            _continueButton = new ButtonView
            {
                ButtonSize = AppButtonSize.Small,
                Text = "",
                Variant = AppButtonVariant.Secondary,
                RightGlyph = Icons.ArrowForward,
                HorizontalOptions = LayoutOptions.End,
            };

            var stack = new VerticalStackLayout { Spacing = 16 };
            stack.Children.Add(topRow);
            stack.Children.Add(_progress);
            stack.Children.Add(_continueButton);

            Content = stack;
            RefreshTheme();
        }

        public string Title
        {
            set
            {
                _titleLabel.Text = value;
            }
        }

        public string? Subtitle
        {
            set
            {
                _subtitleLabel.Text = value;
                _subtitleLabel.IsVisible = !string.IsNullOrEmpty(value);
            }
        }

        public double Progress
        {
            set
            {
                _progress.Progress = value;
                _pillLabel.Text = $"{Math.Round(value)}%";
            }
        }

        public ICommand? ContinueCommand
        {
            set => _continueButton.Command = value;
        }

        public bool HasSubtitle => _subtitleLabel.IsVisible;

        public void RefreshTheme()
        {
            Stroke = ThemeColors.Get("Border");
            BackgroundColor = ThemeColors.Get("Surface");
        }
    }
}