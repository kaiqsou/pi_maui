using System.Windows.Input;

namespace EuropeanMaui.Controls
{
    /// <summary>
    /// Wrapper acionável com feedback de toque (escala), equivalente ao PressableScale do front-end.
    /// Conteúdo é declarado como filho no XAML de uso.
    /// </summary>
    public class PressableScale : ContentView
    {
        private const double DefaultScale = 0.96f;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command), typeof(ICommand), typeof(PressableScale), null);

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            nameof(CommandParameter), typeof(object), typeof(PressableScale), null);

        public static readonly BindableProperty ScaleToProperty = BindableProperty.Create(
            nameof(ScaleTo), typeof(double), typeof(PressableScale), DefaultScale);

        public static readonly BindableProperty EnabledProperty = BindableProperty.Create(
            nameof(Enabled), typeof(bool), typeof(PressableScale), true);

        public PressableScale()
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            GestureRecognizers.Add(tap);

            var pointer = new PointerGestureRecognizer();
            pointer.PointerPressed += OnPointerPressed;
            pointer.PointerReleased += OnPointerReleased;
            pointer.PointerExited += OnPointerExited;
            GestureRecognizers.Add(pointer);
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public double ScaleTo
        {
            get => (double)GetValue(ScaleToProperty);
            set => SetValue(ScaleToProperty, value);
        }

        public bool Enabled
        {
            get => (bool)GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            if (!Enabled) return;
            await ScaleToAsync(ScaleTo, 60);
            await ScaleToAsync(1, 120);
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);
        }

        private async void OnPointerPressed(object? sender, PointerEventArgs e)
        {
            if (Enabled) await ScaleToAsync(ScaleTo, 60);
        }

        private async void OnPointerReleased(object? sender, PointerEventArgs e)
        {
            await ScaleToAsync(1, 120);
        }

        private async void OnPointerExited(object? sender, PointerEventArgs e)
        {
            await ScaleToAsync(1, 120);
        }

        private Task ScaleToAsync(double scale, uint length)
        {
            var anim = new Animation(v => this.Scale = v, this.Scale, scale, Easing.CubicOut);
            var tcs = new TaskCompletionSource<bool>();
            anim.Commit(this, "pressableSize", 16, length, finished: (_, _) => tcs.SetResult(true));
            return tcs.Task;
        }
    }
}