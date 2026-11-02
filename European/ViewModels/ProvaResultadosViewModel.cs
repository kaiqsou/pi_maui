using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ResultadoAlunoItem
    {
        public ResultadoProva Resultado { get; init; } = new();
        public string Nome { get; init; } = string.Empty;
        public string Iniciais { get; init; } = "?";
        public string Detalhe { get; init; } = string.Empty;
        public bool Corrigida => Resultado.Corrigida;
        public bool Aprovado { get; init; }
        public string NotaTexto => Corrigida ? Resultado.NotaObtida.ToString("0.0") : "A corrigir";
    }

    /// <summary>
    /// Entregas de uma prova. Enquanto houver entregas a corrigir, a lista é às cegas ("Resposta #3"),
    /// sem nome nem horário; os nomes só aparecem quando todas estiverem corrigidas.
    /// </summary>
    public class ProvaResultadosViewModel : BaseViewModel
    {
        private Prova? _prova;
        private string _resumo = string.Empty;
        private bool _modoCego;

        public ProvaResultadosViewModel()
        {
            AbrirCommand = new Command<ResultadoAlunoItem>(async item => await AbrirAsync(item));
        }

        public ObservableCollection<ResultadoAlunoItem> Resultados { get; } = new();

        public string Resumo { get => _resumo; private set => SetProperty(ref _resumo, value); }
        public bool ModoCego { get => _modoCego; private set => SetProperty(ref _modoCego, value); }
        public bool SemEntregas => Resultados.Count == 0;

        public ICommand AbrirCommand { get; }

        public Task CarregarAsync(Prova prova) => ExecutarAsync(async () =>
        {
            // relê a prova: as questões podem ter mudado desde que a lista foi aberta
            _prova = await ProvaRepositorio.PorIdAsync(prova.Id) ?? prova;
            Title = _prova.Titulo;

            var resultados = await ResultadoRepositorio.DaProvaAsync(_prova.Id);
            var pendentes = resultados.Count(r => !r.Corrigida);
            ModoCego = pendentes > 0;

            Resultados.Clear();
            if (ModoCego)
            {
                var anonimos = ProvaService.OrdemAnonima(resultados);
                for (var i = 0; i < anonimos.Count; i++)
                {
                    var r = anonimos[i];
                    Resultados.Add(new ResultadoAlunoItem
                    {
                        Resultado = r,
                        Nome = $"Resposta #{i + 1}",
                        Iniciais = $"#{i + 1}",
                        Detalhe = r.Corrigida ? "Corrigida" : DetalheACorrigir(_prova, r),
                        Aprovado = r.Corrigida && r.NotaObtida >= _prova.NotaCorte
                    });
                }

                Resumo = $"Correção às cegas: {pendentes} de {resultados.Count} entrega(s) a corrigir. " +
                         "Os nomes dos alunos aparecem quando todas estiverem corrigidas.";
            }
            else
            {
                var alunos = (await UsuarioRepositorio.PorIdsAsync(resultados.Select(r => r.AlunoId))).ToDictionary(u => u.Id);
                foreach (var r in resultados.OrderByDescending(r => r.NotaObtida))
                {
                    var aluno = alunos.GetValueOrDefault(r.AlunoId);
                    Resultados.Add(new ResultadoAlunoItem
                    {
                        Resultado = r,
                        Nome = aluno?.Nome ?? "Aluno removido",
                        Iniciais = aluno?.Iniciais ?? "?",
                        Detalhe = $"Entregue em {r.DataRealizacao:dd/MM/yyyy HH:mm}" + (r.TempoEsgotado ? " · tempo esgotado" : string.Empty),
                        Aprovado = r.NotaObtida >= _prova.NotaCorte
                    });
                }

                Resumo = resultados.Count == 0
                    ? "Nenhum aluno fez esta prova ainda."
                    : $"Todas corrigidas · {resultados.Count} entrega(s) · média {resultados.Average(r => r.NotaObtida):0.0} · " +
                      $"{Resultados.Count(r => r.Aprovado)} aprovado(s)";
            }

            OnPropertyChanged(nameof(SemEntregas));
        });

        public Task RecarregarAsync() => _prova == null ? Task.CompletedTask : CarregarAsync(_prova);

        private static string DetalheACorrigir(Prova prova, ResultadoProva resultado)
        {
            var dissertativas = prova.Questoes.Count(q => q.IsDissertativa);
            var alternativas = prova.Questoes.Count - dissertativas;
            var partes = new List<string>();
            if (alternativas > 0)
                partes.Add($"{resultado.Acertos}/{alternativas} alternativas certas");
            if (dissertativas > 0)
                partes.Add($"{dissertativas} dissertativa(s) para corrigir");
            return string.Join(" · ", partes);
        }

        private async Task AbrirAsync(ResultadoAlunoItem? item)
        {
            if (item == null || _prova == null)
                return;

            await Shell.Current.GoToAsync("correcao", new Dictionary<string, object>
            {
                ["Prova"] = _prova,
                ["ResultadoId"] = item.Resultado.Id,
                ["Rotulo"] = item.Nome
            });
        }
    }
}
