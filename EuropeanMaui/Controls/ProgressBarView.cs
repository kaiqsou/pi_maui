using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>
    /// Barra de progresso customizada (trilho + preenchimento arredondados), equivalente
    /// ao ProgressBar do front-end. <see cref="ProgressProperty"/> aceita 0..100.
    /// </summary>
    public class ProgressBarView : Grid
    {
        private readonly Border _track;
        private readonly BoxView _fill;

        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
            nameof(Progress), typeof(double), typeof(ProgressBarView), 0d,
            propertyChanged: (b, _, newValue) => ((ProgressBarView)b).ApplyProgress((double)newValue));

        public static readonly BindableProperty FillColorKeyProperty = BindableProperty.Create(
            nameof(FillColorKey), typeof(string), typeof(ProgressBarView), "Primary",
            propertyChanged: (b, _, _) => ((ProgressBarView)b).RefreshTheme());

        public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
            nameof(TrackColor), typeof(Color), typeof(ProgressBarView), null);

        public ProgressBarView()
        {
            HeightRequest = 8;
            _track = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                HeightRequest = 8,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Gray,
            };
            _fill = new BoxView
            {
                Color = Colors.Gray,
                CornerRadius = 4,
                WidthRequest = 0,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Fill,
                ZIndex = 1,
            };
            Children.Add(_track);
            Children.Add(_fill);

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += OnThemeChanged;
            else
                AppServices.ServicesReady += OnServicesReady;
            RefreshTheme();
        }

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public string FillColorKey
        {
            get => (string)GetValue(FillColorKeyProperty);
            set => SetValue(FillColorKeyProperty, value);
        }

        public Color? TrackColor
        {
            get => (Color?)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        public void RefreshTheme()
        {
            if (_track == null || _fill == null) return;
            _track.BackgroundColor = TrackColor ?? ThemeColors.Get("SurfaceAlt");
            _fill.Color = Progress >= 100 ? ThemeColors.Get("Success") : ThemeColors.Get(FillColorKey);
        }

        private void ApplyProgress(double progress)
        {
            // Nada além de invalidar o layout; o tamanho do preenchimento é calculado
            // em OnSizeAllocated para acompanhar a largura real do contêiner.
            if (_fill != null)
                _fill.InvalidateMeasure();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (_fill == null) return;
            var p = Math.Clamp(Progress, 0, 100);
            _fill.WidthRequest = width * p / 100d;
        }

        private void OnThemeChanged() => Dispatcher.Dispatch(RefreshTheme);

        private void OnServicesReady() => OnThemeChanged();
    }
}