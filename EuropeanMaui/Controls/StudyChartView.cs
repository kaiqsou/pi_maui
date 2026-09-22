using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Gráfico de atividade de estudo dos últimos 7 dias (equivalente ao StudyChart do front-end).</summary>
    public class StudyChartView : Border
    {
        private readonly Label _summary;
        private readonly Label _streak;
        private readonly List<Border> _boxes = new();
        private readonly List<Label> _dayLabels = new();

        public StudyChartView()
        {
            StrokeThickness = 1;
            Stroke = ThemeColors.Get("Border");
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = Colors.Transparent;
            Padding = new Thickness(20);

            _summary = new Label
            {
                FontSize = 13,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
            };
            _streak = new Label
            {
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
            };

            var streakRow = new Grid { ColumnSpacing = 6 };
            streakRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            streakRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            streakRow.Add(new GlyphLabel
            {
                Glyph = Icons.Flash,
                FontSize = 16,
                TextColor = ThemeColors.Get("Warning"),
                VerticalOptions = LayoutOptions.Center,
            }, 0, 0);
            streakRow.AddAt(_streak, 1, 0);

            var headerRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };
            headerRow.AddAt(_summary, 0, 0);
            headerRow.AddAt(streakRow, 1, 0);

            var grid = new Grid { ColumnSpacing = 5, RowSpacing = 5 };
            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            for (int i = 0; i < 7; i++)
            {
                var dayLabel = new Label
                {
                    FontSize = 10,
                    FontFamily = "OpenSansBold",
                    TextColor = ThemeColors.Get("TextMuted"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                };
                var box = new Border
                {
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    BackgroundColor = ThemeColors.Get("SurfaceAlt"),
                    HeightRequest = 40,
                    Content = dayLabel,
                };
                grid.Add(box, i, 0);
                _boxes.Add(box);
                _dayLabels.Add(dayLabel);
            }

            var text = new Label
            {
                Text = AppText.Dashboard.StudyTitle,
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("TextMuted"),
                CharacterSpacing = 0.4,
            };

            var stack = new VerticalStackLayout { Spacing = 16 };
            stack.Children.Add(text);
            stack.Children.Add(headerRow);
            stack.Children.Add(grid);

            Content = stack;
        }

        public void SetDays(IEnumerable<StudyDay> days)
        {
            var list = days.ToList();
            int active = list.Count(d => d.Logged);
            _summary.Text = AppText.Dashboard.StudySummary(active, list.Count);

            int streak = CountStreak(list);
            _streak.Text = streak > 0 ? AppText.Dashboard.StudyStreak(streak) : AppText.Dashboard.StudyNoStreak;
            if (streak == 0)
                _streak.TextColor = ThemeColors.Get("Warning");
        }

        public void SetDayColumn(int index, string dayText, bool logged, bool isToday)
        {
            if (index < 0 || index >= _boxes.Count) return;
            _dayLabels[index].Text = dayText;
            _boxes[index].BackgroundColor = isToday
                ? ThemeColors.Get("Success")
                : logged
                    ? ThemeColors.Get("Accent")
                    : ThemeColors.Get("SurfaceAlt");
            _dayLabels[index].TextColor = isToday || logged ? Colors.White : ThemeColors.Get("TextMuted");
        }

        private static int CountStreak(IReadOnlyList<StudyDay> days)
        {
            int streak = 0;
            for (int i = days.Count - 1; i >= 0; i--)
            {
                if (days[i].Logged) streak++;
                else break;
            }
            return streak;
        }
    }
}