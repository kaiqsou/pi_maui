using System.Collections.ObjectModel;
using European.Data;
using European.Models;

namespace European.ViewModels
{
    public class ResultadoAlunoItem
    {
        public string Nome { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Iniciais { get; init; } = "?";
        public double Nota { get; init; }
        public string Detalhe { get; init; } = string.Empty;
        public bool Aprovado { get; init; }
    }

    public class ProvaResultadosViewModel : BaseViewModel
    {
        private string _resumo = string.Empty;

        public ObservableCollection<ResultadoAlunoItem> Resultados { get; } = new();

        public string Resumo { get => _resumo; private set => SetProperty(ref _resumo, value); }

        public Task CarregarAsync(Prova prova) => ExecutarAsync(async () =>
        {
            Title = prova.Titulo;
            var resultados = await ResultadoRepositorio.DaProvaAsync(prova.Id);
            var alunos = (await UsuarioRepositorio.PorIdsAsync(resultados.Select(r => r.AlunoId))).ToDictionary(u => u.Id);

            Resultados.Clear();
            foreach (var resultado in resultados)
            {
                var aluno = alunos.GetValueOrDefault(resultado.AlunoId);
                Resultados.Add(new ResultadoAlunoItem
                {
                    Nome = aluno?.Nome ?? "Aluno removido",
                    Email = aluno?.Email ?? string.Empty,
                    Iniciais = aluno?.Iniciais ?? "?",
                    Nota = resultado.NotaObtida,
                    Detalhe = $"{resultado.Acertos}/{resultado.TotalQuestoes} acertos · {resultado.DataRealizacao:dd/MM/yyyy HH:mm}" +
                              (resultado.TempoEsgotado ? " · tempo esgotado" : string.Empty),
                    Aprovado = resultado.NotaObtida >= prova.NotaCorte
                });
            }

            Resumo = resultados.Count == 0
                ? "Nenhum aluno fez esta prova ainda."
                : $"{resultados.Count} entrega(s) · média {resultados.Average(r => r.NotaObtida):0.0} · " +
                  $"{Resultados.Count(r => r.Aprovado)} aprovado(s)";
        });
    }
}
