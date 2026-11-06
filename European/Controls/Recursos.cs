namespace European.Controls
{
    /// <summary>Acesso aos estilos e cores declarados em Resources/Styles (para controles criados em C#).</summary>
    internal static class Recursos
    {
        public static Style? Estilo(string chave)
            => Application.Current?.Resources.TryGetValue(chave, out var valor) == true ? valor as Style : null;

        /// <summary>Liga a propriedade a uma cor do tema (acompanha a troca entre claro e escuro) e devolve o elemento.</summary>
        public static T ComCor<T>(T elemento, BindableProperty propriedade, string chave) where T : Element
        {
            elemento.SetDynamicResource(propriedade, chave);
            return elemento;
        }
    }
}
