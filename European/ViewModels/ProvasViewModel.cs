using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Prova liberada para o aluno, com o prazo dele.</summary>
    public class ProvaItem
    {
        public ProvaDoAluno Agendada { get; init; } = null!;
        public Prova Prova => Agendada.Prova;
        public string Titulo => Prova.Titulo;
        public string MateriaNome => Prova.Materia?.Nome ?? string.Empty;
        public string Prazo => Expirada ? $"Encerrou em {Agendada.PrazoTexto}" : $"Até {Agendada.PrazoTexto}";
        public string Duracao => Prova.DuracaoTexto;
        public string QuestoesTexto => $"{Prova.Questoes.Count} questões";
        public string ResumoTexto => $"{QuestoesTexto} · {Duracao}";
        public double NotaCorte => Prova.NotaCorte;
        public bool Expirada => Agendada.Encerrada;
    }

    /// <summary>Aba "Provas" do aluno: as disponíveis para fazer e as já realizadas (com nota depois da correção).</summary>
    public partial class ProvasViewModel : BaseViewModel
    {
        public ProvasViewModel()
        {
            Title = "Provas";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiDisponiveis), nameof(MostrarVazioDisponiveis))]
        public partial ObservableCollection<ProvaItem> Disponiveis { get; private set; } = new();

        /// <summary>Disponiveis mostrados aos poucos (5 por vez).</summary>
        public ListaIncremental<ProvaItem> DisponiveisLista => _disponiveisLista ??= new(5, Disponiveis);
        private ListaIncremental<ProvaItem>? _disponiveisLista;

        partial void OnDisponiveisChanged(ObservableCollection<ProvaItem> value) => DisponiveisLista.Fonte = value;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiRealizadas), nameof(MostrarVazioRealizadas))]
        public partial ObservableCollection<ResultadoItem> Realizadas { get; private set; } = new();

        /// <summary>Realizadas mostrados aos poucos (5 por vez).</summary>
        public ListaIncremental<ResultadoItem> RealizadasLista => _realizadasLista ??= new(5, Realizadas);
        private ListaIncremental<ResultadoItem>? _realizadasLista;

        partial void OnRealizadasChanged(ObservableCollection<ResultadoItem> value) => RealizadasLista.Fonte = value;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarDisponiveis), nameof(MostrarRealizadas), nameof(MostrarVazioDisponiveis), nameof(MostrarVazioRealizadas))]
        public partial string AbaSelecionada { get; set; } = "disponiveis";

        public bool MostrarDisponiveis => AbaSelecionada == "disponiveis";
        public bool MostrarRealizadas => AbaSelecionada == "realizadas";
        public bool PossuiDisponiveis => Disponiveis.Count > 0;
        public bool PossuiRealizadas => Realizadas.Count > 0;
        public bool MostrarVazioDisponiveis => MostrarDisponiveis && !PossuiDisponiveis;
        public bool MostrarVazioRealizadas => MostrarRealizadas && !PossuiRealizadas;

        /// <summary>Quando a próxima prova do calendário será liberada (vazio se não houver).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VazioDisponiveisTexto))]
        public partial string ProximaLiberacao { get; private set; } = string.Empty;

        public string VazioDisponiveisTexto => string.IsNullOrEmpty(ProximaLiberacao)
            ? "No momento não há avaliações abertas para você."
            : $"A próxima prova será liberada em {ProximaLiberacao}. Você será avisado no sino.";

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null)
            {
                Disponiveis = new();
                Realizadas = new();
                return;
            }

            var matriculas = await MatriculaRepositorio.AtivasDoAlunoAsync(aluno.Id);
            var agenda = await ProvaService.AgendaAsync(matriculas);
            var resultados = await ResultadoRepositorio.DoAlunoAsync(aluno.Id);
            var respondidas = resultados.Select(r => r.ProvaId).ToHashSet();

            // provas já feitas que não estão mais no calendário (ex.: despublicadas) são buscadas de uma vez
            var porId = agenda.ToDictionary(p => p.Prova.Id, p => p.Prova);
            foreach (var prova in await ProvaRepositorio.PorIdsAsync(respondidas.Where(id => !porId.ContainsKey(id))))
                porId[prova.Id] = prova;

            // liberadas e ainda não feitas: as abertas primeiro (prazo mais próximo), depois as encerradas
            Disponiveis = new(agenda
                .Where(p => p.Liberada && !respondidas.Contains(p.Prova.Id))
                .OrderBy(p => p.Encerrada).ThenBy(p => p.Encerrada ? -p.PrazoAte.Ticks : p.PrazoAte.Ticks)
                .Select(p => new ProvaItem { Agendada = p }));
            Realizadas = new(resultados.Select(r => ResultadoItem.De(r, porId.GetValueOrDefault(r.ProvaId))));
            ProximaLiberacao = agenda.FirstOrDefault(p => !p.Liberada)?.LiberacaoTexto ?? string.Empty;
        });

        [RelayCommand]
        private void SelecionarAba(string aba) => AbaSelecionada = aba;

        /// <summary>Prova corrigida: abre a revisão com os erros e os comentários do professor.</summary>
        [RelayCommand]
        private async Task RevisarAsync(ResultadoItem? item)
        {
            if (item == null)
                return;

            if (!item.PodeRever)
            {
                await AvisarAsync("Prova", item.Corrigida ? "Esta prova não está mais disponível." : "Aguardando a correção do professor.");
                return;
            }

            await Navegacao.IrAsync(Navegacao.ProvaRevisao, ("ResultadoId", item.Resultado.Id), ("ProvaId", item.Prova!.Id));
        }

        [RelayCommand]
        private async Task IniciarAsync(ProvaItem? item)
        {
            if (item == null)
                return;

            if (item.Expirada)
            {
                await AvisarAsync("Prova encerrada", "O prazo desta prova terminou.");
                return;
            }

            if (await ConfirmarAsync("Deseja iniciar a prova?", "Iniciar"))
                await Navegacao.IrAsync(Navegacao.ProvaRealizar, ("Prova", item.Prova));
        }
    }
}
