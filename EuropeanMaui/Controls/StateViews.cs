using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Esqueleto pulsante (equivalente ao Skeleton do front-end).</summary>
    public class SkeletonView : Border
    {
        public SkeletonView(double height, double width = double.NaN, double radius = 8)
        {
            StrokeThickness = 0;
            StrokeShape = new RoundRectangle { CornerRadius = radius };
            BackgroundColor = ThemeColors.Get("SurfaceAlt");
            HeightRequest = height;
            if (!double.IsNaN(width) && width >= 0)
                WidthRequest = width;
            HorizontalOptions = LayoutOptions.Fill;
            Margin = 0;
        }

        public void Start()
        {
            this.Opacity = 0.45;
            var animation = new Animation(v => Opacity = v, 0.45, 1, Easing.CubicInOut);
            animation.Commit(this, "skeleton", 16, 1400, repeat: () => true);
        }

        public static void StartPulse(IView view)
        {
            if (view is VisualElement ve)
                new SkeletonPulse(ve).Start();
        }
    }

    internal class SkeletonPulse
    {
        private readonly VisualElement _view;
        public SkeletonPulse(VisualElement view) => _view = view;

        public void Start()
        {
            _view.Opacity = 0.45;
            new Animation(v => _view.Opacity = v, 0.45, 1, Easing.CubicInOut)
                .Commit(_view, "pulse", 16, 1400, repeat: () => true);
        }
    }

    /// <summary>Cartão esqueleto com linhas (equivalente ao SkeletonCard do front-end).</summary>
    public class SkeletonCardView : Border
    {
        public SkeletonCardView(int lines = 3)
        {
            StrokeThickness = 1;
            Stroke = ThemeColors.Get("Border");
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = ThemeColors.Get("Surface");
            Padding = new Thickness(20);

            var stack = new VerticalStackLayout { Spacing = 12 };
            for (int i = 0; i < lines; i++)
            {
                var line = new SkeletonView(14, radius: 6)
                {
                    HorizontalOptions = i == lines - 1 ? LayoutOptions.Start : LayoutOptions.Fill,
                };
                if (i == lines - 1)
                    line.WidthRequest = 120;
                stack.Children.Add(line);
            }
            Content = stack;
        }
    }

    /// <summary>Spinner + mensagem (equivalente ao LoadingState do front-end).</summary>
    public class LoadingStateView : VerticalStackLayout
    {
        public LoadingStateView(string message)
        {
            Spacing = 16;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Padding = new Thickness(32);

            Children.Add(new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                HeightRequest = 36,
                WidthRequest = 36,
                Color = ThemeColors.Get("Primary"),
            });
            Children.Add(new Label
            {
                Text = message,
                FontSize = 14,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            });
        }
    }

    /// <summary>Estado vazio (equivalente ao EmptyState do front-end).</summary>
    public class EmptyStateView : VerticalStackLayout
    {
        public EmptyStateView(string title, string? message = null)
        {
            Spacing = 8;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Padding = new Thickness(32);

            var color = ThemeColors.Get("TextMuted");
            Children.Add(new Label
            {
                Text = title,
                FontSize = 18,
                FontFamily = "OpenSansSemibold",
                TextColor = color,
                HorizontalTextAlignment = TextAlignment.Center,
            });
            if (!string.IsNullOrEmpty(message))
            {
                Children.Add(new Label
                {
                    Text = message,
                    FontSize = 14,
                    TextColor = color,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }
        }
    }

    /// <summary>Estado de erro com retry (equivalente ao ErrorState do front-end).</summary>
    public class ErrorStateView : VerticalStackLayout
    {
        private readonly ButtonView _retry;

        public ErrorStateView(string message, Action? onRetry = null)
        {
            Spacing = 16;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Padding = new Thickness(32);

            Children.Add(new Label
            {
                Text = AppText.Common.ErrorOccurred,
                FontSize = 18,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Danger"),
                HorizontalTextAlignment = TextAlignment.Center,
            });
            Children.Add(new Label
            {
                Text = message,
                FontSize = 14,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            });

            _retry = new ButtonView
            {
                Text = AppText.Common.Retry,
                Variant = AppButtonVariant.Primary,
                ButtonSize = AppButtonSize.Medium,
                Command = new RelayCommand(() => onRetry?.Invoke()),
            };
            if (onRetry != null)
                Children.Add(_retry);
        }
    }
}