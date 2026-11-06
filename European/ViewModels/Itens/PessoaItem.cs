using System.Windows.Input;
using European.Models;

namespace European.ViewModels
{
    /// <summary>Linha de uma lista de pessoas (aluno, professor ou candidato).</summary>
    public class PessoaItem
    {
        public Usuario Usuario { get; init; } = new();

        /// <summary>Preenchido quando o item é uma solicitação de matrícula (o candidato ainda não é usuário).</summary>
        public SolicitacaoMatricula? Solicitacao { get; init; }
        public string Nome => Usuario.Nome;
        public string Email => Usuario.Email;
        public string Iniciais => Usuario.Iniciais;
        public string Status { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
        public bool Destaque { get; init; }
        public bool Alerta { get; init; }
        public ICommand? Abrir { get; set; }
    }
}
