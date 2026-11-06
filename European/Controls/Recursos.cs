namespace European.Controls
{
    /// <summary>Acesso aos estilos e cores declarados em Resources/Styles (para controles criados em C#).</summary>
    internal static class Recursos
    {
        public static Style? Estilo(string chave) => Obter(chave) as Style;

        public static Color Cor(string chave, Color? padrao = null) => Obter(chave) as Color ?? padrao ?? Colors.Gray;

        private static object? Obter(string chave)
            => Application.Current?.Resources.TryGetValue(chave, out var valor) == true ? valor : null;
    }
}
