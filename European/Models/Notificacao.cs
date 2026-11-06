namespace European.Models
{
    /// <summary>Aviso para um usuário (aluno, professor ou coordenação), exibido no sino das telas iniciais.</summary>
    public class Notificacao : IEntidade
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>Quem recebe. Avisos "para a coordenação" geram uma notificação para cada coordenador.</summary>
        public string UsuarioId { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime CriadaEm { get; set; } = DateTime.Now;
        public bool Lida { get; set; }
    }
}
