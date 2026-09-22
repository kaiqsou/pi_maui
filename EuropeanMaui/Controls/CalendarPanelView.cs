using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Painel de calendário / prazos (equivalente ao CalendarPanel do front-end).</summary>
    public class CalendarPanelView : Border
    {
        private readonly VerticalStackLayout _dueHost;

        public CalendarPanelView(Action<DueItem>? onDueTap, Action? onFullCalendar = null)
        {
            StrokeThickness = 1;
            Stroke = ThemeColors.Get("Border");
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = Colors.Transparent;
            Padding = 0;

            var header = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                },
                ColumnSpacing = 10,
            };
            var iconBox = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = ThemeColors.Get("SurfaceAlt"),
                WidthRequest = 34,
                HeightRequest = 34,
                Content = new GlyphLabel
                {
                    Glyph = Icons.Calendar,
                    FontSize = 17,
                    TextColor = ThemeColors.Get("Accent"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };
            var title = new Label
            {
                Text = AppText.Dashboard.CalendarTitle,
                FontSize = 15,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                VerticalOptions = LayoutOptions.Center,
            };
            header.AddAt(iconBox, 0, 0);
            header.AddAt(title, 1, 0);

            var rowLabel = new Label
            {
                Text = AppText.Dashboard.DueSoon,
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("TextMuted"),
                CharacterSpacing = 0.4,
            };
            var emptyLabel = new Label
            {
                Text = AppText.Dashboard.NoDueItems,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                IsVisible = false,
            };

            _dueHost = new VerticalStackLayout { Spacing = 8 };

            var section = new VerticalStackLayout { Spacing = 10 };
            section.Children.Add(rowLabel);
            section.Children.Add(emptyLabel);
            section.Children.Add(_dueHost);

            var eventsLabel = new Label
            {
                Text = AppText.Dashboard.EventsTitle,
                FontSize = 12,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("TextMuted"),
                CharacterSpacing = 0.4,
            };
            var noEvents = new Label
            {
                Text = AppText.Dashboard.NoEvents,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
            };
            var verticalDivider = new BoxView { HeightRequest = 1, Color = ThemeColors.Get("Border"), Margin = new Thickness(0, 4) };

            var eventsSection = new VerticalStackLayout { Spacing = 8 };
            eventsSection.Children.Add(eventsLabel);
            eventsSection.Children.Add(noEvents);

            var calendarButton = new ButtonView
            {
                Text = AppText.Dashboard.ViewFullCalendar,
                Variant = AppButtonVariant.Outline,
                ButtonSize = AppButtonSize.Small,
                LeftGlyph = Icons.CalendarOutline,
                HorizontalOptions = LayoutOptions.Start,
            };
            if (onFullCalendar != null)
                calendarButton.Command = new RelayCommand(onFullCalendar);

            var stack = new VerticalStackLayout { Spacing = 20 };
            stack.Children.Add(header);
            stack.Children.Add(section);
            stack.Children.Add(verticalDivider);
            stack.Children.Add(eventsSection);
            stack.Children.Add(calendarButton);

            Padding = new Thickness(20);
            Content = stack;

            OnDueTap = onDueTap;
        }

        public Action<DueItem>? OnDueTap { get; }

        public void SetDueItems(IEnumerable<DueItem> items)
        {
            var list = items.ToList();
            _dueHost.Children.Clear();
            if (list.Count == 0)
                _dueHost.IsVisible = false;
            else
            {
                _dueHost.IsVisible = true;
                foreach (var item in list)
                    _dueHost.Children.Add(CreateDueRow(item));
            }
        }

        private Border CreateDueRow(DueItem item)
        {
            bool isExam = item.Type == DueItemType.Exam;
            var color = isExam ? ThemeColors.Get("Warning") : ThemeColors.Get("PrimaryDark");
            var badge = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                BackgroundColor = color,
                WidthRequest = 34,
                HeightRequest = 34,
                Content = new GlyphLabel
                {
                    Glyph = isExam ? Icons.DocumentText : Icons.School,
                    FontSize = 17,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };

            var module = new Label
            {
                FontSize = 13,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var meta = new Label
            {
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var textCol = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            textCol.Children.Add(module);
            textCol.Children.Add(meta);

            var viewMore = new Label
            {
                Text = AppText.Dashboard.ViewMore,
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Primary"),
                VerticalOptions = LayoutOptions.Center,
            };

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            row.AddAt(badge, 0, 0);
            row.AddAt(textCol, 1, 0);
            row.AddAt(viewMore, 2, 0);

            module.Text = item.ModuleTitle;
            meta.Text = $"{(isExam ? AppText.Dashboard.ExamScheduled : AppText.Dashboard.ActivityPending)} · {(item.DueInDays <= 1 ? AppText.Dashboard.DueInOneDay : AppText.Dashboard.DueInDays(item.DueInDays))}";

            var card = new Border
            {
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(12, 10),
                Content = row,
            };

            var pressable = new PressableScale { Content = card, HorizontalOptions = LayoutOptions.Fill };
            pressable.Command = new RelayCommand<DueItem>(d => OnDueTap?.Invoke(d));
            pressable.CommandParameter = item;
            return card;
        }
    }
}