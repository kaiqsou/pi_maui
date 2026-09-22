using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Card de módulo (equivalente ao ModuleCard do front-end).</summary>
    public class ModuleCardView : Border
    {
        private readonly Grid _root;
        private readonly ColumnDefinition _chevronCol;
        private readonly Action<Module>? _onTap;
        private Module? _module;

        public ModuleCardView(Action<Module>? onTap = null)
        {
            _onTap = onTap;
            StrokeThickness = 1;
            Stroke = Colors.Transparent;
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = ThemeColors.Get("Surface");
            Padding = new Thickness(16);

            _chevronCol = new ColumnDefinition(GridLength.Auto);
            _root = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    _chevronCol,
                },
                ColumnSpacing = 12,
            };

            var badge = new BadgeView();

            var duration = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalOptions = LayoutOptions.Center,
            };

            var topRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 8,
            };
            topRow.AddAt(badge, 0, 0);
            topRow.AddAt(duration, 2, 0);

            var title = new Label
            {
                FontSize = 15,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var desc = new Label
            {
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2,
            };

            var progress = new ProgressBarView { HeightRequest = 6 };
            var progressLabel = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("TextMuted"),
            };

            var textCol = new VerticalStackLayout { Spacing = 6 };
            textCol.Children.Add(topRow);
            textCol.Children.Add(title);
            textCol.Children.Add(desc);
            textCol.Children.Add(progress);
            textCol.Children.Add(progressLabel);

            _root.AddAt(textCol, 0, 0);

            var chevron = new GlyphLabel
            {
                Glyph = Icons.ChevronForward,
                FontSize = 20,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = false,
            };
            _root.AddAt(chevron, 1, 0);

            var pressable = new PressableScale { Content = _root, HorizontalOptions = LayoutOptions.Fill };
            pressable.Command = new RelayCommand<Module>(m => _onTap?.Invoke(m));
            pressable.CommandParameter = _module;

            Content = pressable;

            Badge = badge;
            Duration = duration;
            Title = title;
            Description = desc;
            Progress = progress;
            ProgressLabel = progressLabel;
        }

        public BadgeView Badge { get; }
        public Label Duration { get; }
        public Label Title { get; }
        public Label Description { get; }
        public ProgressBarView Progress { get; }
        public Label ProgressLabel { get; }

        public Module? Module
        {
            set
            {
                _module = value;
                if (value == null) return;

                if (value.IsCompleted)
                {
                    Badge.Label = AppText.Modulos.ModuleCompleted;
                    Badge.Tone = BadgeTone.Success;
                }
                else if (value.IsInProgress)
                {
                    Badge.Label = AppText.Modulos.ModuleInProgress;
                    Badge.Tone = BadgeTone.Primary;
                }
                else
                {
                    Badge.Label = AppText.Modulos.ModuleNotStarted;
                    Badge.Tone = BadgeTone.Neutral;
                }

                Duration.Text = $"{value.DurationHours}h";
                Title.Text = value.Title;
                Description.Text = value.Description;
                Progress.Progress = value.Progress;

                if (value.IsCompleted)
                    ProgressLabel.Text = $"{value.Progress}% · {AppText.Dashboard.CompletedLabel}";
                else if (value.IsInProgress)
                    ProgressLabel.Text = $"{value.Progress}% {AppText.Modulos.ModuleInProgress}";
                else
                    ProgressLabel.Text = $"{value.Progress}% {AppText.Modulos.ModuleNotStarted}";
            }
        }

        public bool ShowChevron
        {
            set
            {
                _chevronCol.Width = value ? GridLength.Auto : new GridLength(0);
                if (_root.Children.Count > 1 && _root.Children[1] is VisualElement el)
                    el.IsVisible = value;
            }
        }
    }
}