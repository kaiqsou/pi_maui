using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Correção de uma entrega. Às cegas: a tela não mostra nome, e-mail nem horário de entrega.
    /// Alternativas já chegam corrigidas; nas dissertativas o professor dá a nota e pode comentar.
    /// </summary>
    public partial class CorrecaoViewModel : BaseViewModel
    {
        private Prova? _prova;
        private ResultadoProva? _resultado;

        public ObservableCollection<QuestaoCorrecao> Questoes { get; } = new();

        [ObservableProperty]
        public partial string NotaTotalTexto { get; private set; } = string.Empty;

        public bool JaCorrigida => _resultado?.Corrigida == true;
        public string ConcluirTexto => JaCorrigida ? "Salvar correção" : "Concluir correção";

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<Prova>("Prova") is not { } prova || parametros.Obter<string>("ResultadoId") is not { } resultadoId)
                return Task.CompletedTask;

            var rotulo = parametros.Obter<string>("Rotulo") ?? "Correção";
            return ExecutarAsync(async () =>
            {
                _prova = prova;
                _resultado = await ResultadoRepositorio.PorIdAsync(resultadoId);
                Title = rotulo;
                Questoes.Clear();
                if (_resultado == null)
                    return;

                ProvaService.CompletarNotasAntigas(prova, _resultado);
                for (var i = 0; i < prova.Questoes.Count; i++)
                {
                    var item = QuestaoCorrecao.De(prova.Questoes[i], i, _resultado);
                    item.NotaAlterada += AtualizarTotal;
                    Questoes.Add(item);
                }

                AtualizarTotal();
                OnPropertyChanged(nameof(JaCorrigida));
                OnPropertyChanged(nameof(ConcluirTexto));
            });
        }

        /// <summary>Atualiza a nota da entrega enquanto o professor digita as notas das dissertativas.</summary>
        private void AtualizarTotal()
        {
            if (_prova == null || _resultado == null)
                return;

            var nota = ProvaService.CalcularNota(_prova, new ResultadoProva { NotasQuestoes = NotasDigitadas() });
            var faltam = Questoes.Count(q => q.IsDissertativa && q.NotaValor == null);
            NotaTotalTexto = faltam > 0
                ? $"Nota parcial: {nota:0.0} de {_prova.NotaMaxima:0.#} · {faltam} dissertativa(s) sem nota"
                : $"Nota final: {nota:0.0} de {_prova.NotaMaxima:0.#}";
        }

        /// <summary>Notas por questão: as automáticas (alternativas) mais o que está digitado nas dissertativas.</summary>
        private List<double?> NotasDigitadas()
        {
            var notas = _resultado!.NotasQuestoes.ToList();
            while (notas.Count < Questoes.Count)
                notas.Add(null);
            foreach (var q in Questoes.Where(q => q.IsDissertativa))
                notas[q.Indice] = q.NotaValor;
            return notas;
        }

        [RelayCommand]
        private Task Salvar() => SalvarAsync(concluir: false);

        [RelayCommand]
        private Task Concluir() => SalvarAsync(concluir: true);

        private async Task SalvarAsync(bool concluir)
        {
            if (_prova == null || _resultado == null)
                return;

            var invalida = Questoes.FirstOrDefault(q => q.IsDissertativa && !string.IsNullOrWhiteSpace(q.Nota) && q.NotaValor == null);
            if (invalida != null)
            {
                await AvisarAsync("Correção", $"A nota da questão {invalida.Numero} deve ser um número de 0 a 10.");
                return;
            }

            var semNota = Questoes.FirstOrDefault(q => q.IsDissertativa && q.NotaValor == null);
            if (concluir && semNota != null)
            {
                await AvisarAsync("Correção", $"Dê a nota da questão {semNota.Numero} para concluir.");
                return;
            }

            var resultado = _resultado;
            resultado.NotasQuestoes = NotasDigitadas();
            resultado.Comentarios = Questoes.Select(q => q.IsDissertativa ? q.Comentario.Trim() : string.Empty).ToList();

            if (!await ExecutarAsync(() => ProvaService.SalvarCorrecaoAsync(_prova, resultado, concluir)))
                return;

            await AvisarAsync("Correção", concluir
                ? $"Correção concluída. Nota: {resultado.NotaObtida:0.0} de {_prova.NotaMaxima:0.#}. O aluno já pode ver a nota."
                : "Correção salva. Você pode concluir depois.");
            await VoltarAsync();
        }
    }
}
