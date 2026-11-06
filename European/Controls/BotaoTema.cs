using European.Services;

namespace European.Controls
{
    /// <summary>Botão de sol/lua que alterna entre o tema claro e o escuro (TemaService).</summary>
    public class BotaoTema : ContentView
    {
        private const string IconeLua = "";  // dark_mode: mostrado no tema claro
        private const string IconeSol = "";  // light_mode: mostrado no tema escuro

        private readonly Label _icone;

        public BotaoTema()
        {
            _icone = new Label
            {
                Style = Recursos.Estilo("IconPrimary"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var botao = new Border { Style = Recursos.Estilo("BackTile"), WidthRequest = 44, HeightRequest = 44, Content = _icone };
            botao.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(TemaService.Alternar) });
            Content = botao;

            Atualizar();
            Loaded += (_, _) => TemaService.Alterado += Atualizar;
            Unloaded += (_, _) => TemaService.Alterado -= Atualizar;
        }

        private void Atualizar()
        {
            _icone.Text = TemaService.Escuro ? IconeSol : IconeLua;
            SemanticProperties.SetDescription(this, TemaService.Escuro ? "Usar tema claro" : "Usar tema escuro");
        }
    }
}
