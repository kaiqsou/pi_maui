namespace European.Controls
{
    /// <summary>
    /// Botão de alternância (abas internas e filtros): quando <see cref="Ativo"/> é verdadeiro fica com a cor da marca;
    /// senão, volta ao visual do Style dele (GhostButton, Filtro...).
    /// </summary>
    public class BotaoSegmento : Button
    {
        public static readonly BindableProperty AtivoProperty =
            BindableProperty.Create(nameof(Ativo), typeof(bool), typeof(BotaoSegmento), false,
                propertyChanged: (b, _, _) => ((BotaoSegmento)b).Atualizar());

        public bool Ativo { get => (bool)GetValue(AtivoProperty); set => SetValue(AtivoProperty, value); }

        private void Atualizar()
        {
            if (Ativo)
            {
                BackgroundColor = Recursos.Cor("BrandPrimary");
                TextColor = Recursos.Cor("BrandOnPrimaryContainer");
            }
            else
            {
                ClearValue(BackgroundColorProperty);
                ClearValue(TextColorProperty);
            }
        }
    }
}
