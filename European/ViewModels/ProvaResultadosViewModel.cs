using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    /// Visualização de uma prova: dados essenciais e as entregas dos alunos (tocar abre a prova respondida).
    /// Enquanto houver entregas a corrigir, a lista é às cegas ("Resposta #3"), sem nome nem horário;
    /// os nomes só aparecem quando todas estiverem corrigidas. O professor dono da prova pode editá-la.
    /// </summary>
    public partial class ProvaResultadosViewModel : BaseViewModel
    {
        private Prova? _prova;

        /// <summary>Quantas notas aparecem de início (as mais recentes), para a lista não virar uma página enorme.</summary>
        private const int NotasVisiveis = 3;

        /// <summary>Todas as notas da prova (depois da correção); Resultados mostra só as últimas, ou todas se pedido.</summary>
        private List<ResultadoAlunoItem> _notas = new();

        public ObservableCollection<ResultadoAlunoItem> Resultados { get; } = new();

        [ObservableProperty]
        public partial string Resumo { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial bool ModoCego { get; private set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VerTodasTexto))]
        public partial bool MostrarTodas { get; private set; }

        /// <summary>Há mais notas além das 3 mais recentes (botão "Ver todas").</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VerTodasTexto))]
        public partial bool TemMaisNotas { get; private set; }

        public string VerTodasTexto => MostrarTodas ? "Mostrar só as últimas" : $"Ver todas as notas ({_notas.Count})";

        // ---- Dados essenciais da prova ----

        [ObservableProperty]
        public partial string MateriaTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string RegrasTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string PrazoTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string StatusTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial bool Publicada { get; private set; }

        /// <summary>Os professores do curso veem o botão Editar.</summary>
        [ObservableProperty]
        public partial bool PodeEditar { get; private set; }

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _prova = parametros.Obter<Prova>("Prova") ?? _prova;
            parametros.Clear();
            return Task.CompletedTask;
        }

        /// <summary>Recarrega também ao voltar da correção de uma entrega (e revela os nomes se acabou).</summary>
        public override Task AoAparecerAsync() => _prova == null ? Task.CompletedTask : ExecutarAsync(async () =>
        {
            // relê a prova: as questões podem ter mudado desde que a lista foi aberta
            _prova = await ProvaRepositorio.PorIdAsync(_prova.Id) ?? _prova;
            Title = _prova.Titulo;
            await CarregarDadosDaProvaAsync(_prova);

            var resultados = await ResultadoRepositorio.DaProvaAsync(_prova.Id);
            var pendentes = resultados.Count(r => !r.Corrigida);
            ModoCego = pendentes > 0;

            Resultados.Clear();
            _notas = new();
            TemMaisNotas = false;
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
                return;
            }

            // notas das entregas mais recentes primeiro; de início só as 3 últimas aparecem
            var alunos = (await UsuarioRepositorio.PorIdsAsync(resultados.Select(r => r.AlunoId))).ToDictionary(u => u.Id);
            _notas = resultados.OrderByDescending(r => r.DataRealizacao).Select(r =>
            {
                var aluno = alunos.GetValueOrDefault(r.AlunoId);
                return new ResultadoAlunoItem
                {
                    Resultado = r,
                    Nome = aluno?.Nome ?? "Aluno removido",
                    Iniciais = aluno?.Iniciais ?? "?",
                    Detalhe = $"Entregue em {r.DataRealizacao:dd/MM/yyyy HH:mm}" + (r.TempoEsgotado ? " · tempo esgotado" : string.Empty),
                    Aprovado = r.NotaObtida >= _prova.NotaCorte
                };
            }).ToList();
            TemMaisNotas = _notas.Count > NotasVisiveis;
            ExibirNotas();

            Resumo = resultados.Count == 0
                ? "Nenhum aluno fez esta prova ainda."
                : $"Todas corrigidas · {resultados.Count} entrega(s) · média {resultados.Average(r => r.NotaObtida):0.0} · " +
                  $"{_notas.Count(r => r.Aprovado)} aprovado(s)";
        });

        private void ExibirNotas()
        {
            Resultados.Clear();
            foreach (var item in MostrarTodas ? _notas : _notas.Take(NotasVisiveis))
                Resultados.Add(item);
            OnPropertyChanged(nameof(VerTodasTexto));
        }

        [RelayCommand]
        private void AlternarTodas()
        {
            MostrarTodas = !MostrarTodas;
            ExibirNotas();
        }

        private async Task CarregarDadosDaProvaAsync(Prova prova)
        {
            var curso = await CursoRepositorio.PorIdAsync(prova.CursoId);
            var materia = curso?.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId);

            MateriaTexto = $"{materia?.Nome ?? "Matéria removida"} · {curso?.Nome ?? "Curso removido"}";
            RegrasTexto = $"{prova.Questoes.Count} questão(ões) · {prova.DuracaoTexto} · nota máxima {prova.NotaMaxima:0.#} · corte {prova.NotaCorte:0.#}";
            PrazoTexto = $"Prazo: {prova.PrazoDiasTexto} para cada aluno, a partir da liberação";
            Publicada = prova.IsPublicada;
            StatusTexto = prova.IsPublicada ? "Publicada" : "Rascunho";
            // a prova é do curso: qualquer professor dele pode editar
            var usuario = SessionService.UsuarioAtual?.Id;
            PodeEditar = usuario != null && (prova.ProfessorId == usuario || curso?.ProfessorIds.Contains(usuario) == true);
        }

        [RelayCommand]
        private Task Editar() => _prova == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ProvaEdit, ("Prova", _prova));

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

        [RelayCommand]
        private Task Abrir(ResultadoAlunoItem? item)
            => item == null || _prova == null
                ? Task.CompletedTask
                : Navegacao.IrAsync(Navegacao.Correcao, ("Prova", _prova), ("ResultadoId", item.Resultado.Id), ("Rotulo", item.Nome));
    }
}
