using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Aluno revê a prova já corrigida: em cada questão, o que respondeu, se acertou (com a alternativa correta quando
    /// errou) e, nas dissertativas, a nota e o comentário do professor. A resposta esperada não aparece: é do professor.
    /// </summary>
    public partial class ProvaRevisaoViewModel : BaseViewModel
    {
        public ObservableCollection<QuestaoCorrecao> Questoes { get; } = new();

        [ObservableProperty]
        public partial string NotaTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string ResumoTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial bool Aprovado { get; private set; }

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<string>("ResultadoId") is not { } resultadoId)
                return Task.CompletedTask;

            var provaId = parametros.Obter<string>("ProvaId");
            return ExecutarAsync(async () =>
            {
                var resultado = await ResultadoRepositorio.PorIdAsync(resultadoId);
                var prova = provaId == null ? null : await ProvaRepositorio.PorIdAsync(provaId);
                Questoes.Clear();

                // só o próprio aluno, e só depois da correção
                if (resultado == null || prova == null || !resultado.Corrigida || resultado.AlunoId != SessionService.UsuarioAtual?.Id)
                {
                    await AvisarAsync("Prova", "A correção desta prova não está disponível.");
                    await VoltarAsync();
                    return;
                }

                Title = prova.Titulo;
                Aprovado = resultado.NotaObtida >= prova.NotaCorte;
                NotaTexto = $"{resultado.NotaObtida:0.0} de {prova.NotaMaxima:0.#}";

                ProvaService.CompletarNotasAntigas(prova, resultado);
                for (var i = 0; i < prova.Questoes.Count; i++)
                    Questoes.Add(QuestaoCorrecao.De(prova.Questoes[i], i, resultado));

                var alternativas = Questoes.Where(q => q.IsAlternativas).ToList();
                var partes = new List<string> { Aprovado ? "Aprovado" : $"Abaixo da nota de corte ({prova.NotaCorte:0.#})" };
                if (alternativas.Count > 0)
                    partes.Add($"{alternativas.Count(q => q.Acertou)} de {alternativas.Count} alternativas certas");
                ResumoTexto = string.Join(" · ", partes);
            });
        }
    }
}
