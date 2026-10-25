using System.Windows.Input;

namespace European.Controls
{
    /// <summary>Item da barra inferior de abas (ícone Material + texto), destacado quando ativo.</summary>
    public class AbaBotao : ContentView
    {
        public static readonly BindableProperty IconeProperty =
            BindableProperty.Create(nameof(Icone), typeof(string), typeof(AbaBotao), string.Empty, propertyChanged: (b, _, _) => ((AbaBotao)b).Atualizar());

        public static readonly BindableProperty TextoProperty =
            BindableProperty.Create(nameof(Texto), typeof(string), typeof(AbaBotao), string.Empty, propertyChanged: (b, _, _) => ((AbaBotao)b).Atualizar());

        public static readonly BindableProperty AtivaProperty =
            BindableProperty.Create(nameof(Ativa), typeof(bool), typeof(AbaBotao), false, propertyChanged: (b, _, _) => ((AbaBotao)b).Atualizar());

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AbaBotao));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AbaBotao));

        private readonly Label _icone = new() { FontFamily = "MaterialIcons", FontSize = 22, HorizontalOptions = LayoutOptions.Center };
        private readonly Label _texto = new() { FontSize = 11, HorizontalOptions = LayoutOptions.Center };

        public AbaBotao()
        {
            Content = new VerticalStackLayout { Spacing = 2, Children = { _icone, _texto } };
            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (Command?.CanExecute(CommandParameter) == true)
                        Command.Execute(CommandParameter);
                })
            });
            Atualizar();
        }

        public string Icone { get => (string)GetValue(IconeProperty); set => SetValue(IconeProperty, value); }
        public string Texto { get => (string)GetValue(TextoProperty); set => SetValue(TextoProperty, value); }
        public bool Ativa { get => (bool)GetValue(AtivaProperty); set => SetValue(AtivaProperty, value); }
        public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
        public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

        private void Atualizar()
        {
            var cor = Cor(Ativa ? "BrandPrimary" : "OnSurfaceVariant");
            _icone.Text = Icone;
            _icone.TextColor = cor;
            _texto.Text = Texto;
            _texto.TextColor = cor;
            _texto.FontFamily = Ativa ? "OpenSansSemibold" : "OpenSansRegular";
        }

        private static Color Cor(string chave)
            => Application.Current?.Resources.TryGetValue(chave, out var valor) == true && valor is Color cor ? cor : Colors.Gray;
    }
}
