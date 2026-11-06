using Microsoft.Maui.Controls.Internals;

namespace European.Controls
{
    /// <summary>
    /// Botão de alternância (abas internas e filtros): quando <see cref="Ativo"/> é verdadeiro fica com a cor da marca;
    /// senão, volta ao visual do Style dele (GhostButton, Filtro...).
    /// </summary>
    public class BotaoSegmento : Button
    {
        public static readonly BindableProperty AtivoProperty =
            BindableProperty.Create(nameof(Ativo), typeof(bool), typeof(BotaoSegmento), false);

        public BotaoSegmento()
        {
            // Um Trigger desfaz os próprios Setters quando deixa de valer, devolvendo as cores do Style.
            // As cores são do tema (DynamicResource), então acompanham a troca entre claro e escuro.
            Triggers.Add(new Trigger(typeof(BotaoSegmento))
            {
                Property = AtivoProperty,
                Value = true,
                Setters =
                {
                    new Setter { Property = BackgroundColorProperty, Value = new DynamicResource("BrandPrimary") },
                    new Setter { Property = TextColorProperty, Value = new DynamicResource("BrandOnPrimaryContainer") }
                }
            });
        }

        public bool Ativo { get => (bool)GetValue(AtivoProperty); set => SetValue(AtivoProperty, value); }
    }
}
