namespace European.Controls
{
    /// <summary>
    /// Entry de senha usada pelo CampoSenha. No Windows ela é desenhada com o PasswordBox nativo
    /// (Platforms/Windows/CaixaSenhaHandler): o Entry padrão do MAUI é uma caixa de texto comum que troca cada
    /// letra por bolinha só depois de ela aparecer, então digitando rápido dava para ver a senha por um instante.
    /// Nas outras plataformas ela é um Entry normal. IsPassword = false mostra a senha (olhinho).
    /// </summary>
    public class CaixaSenha : Entry
    {
    }
}
