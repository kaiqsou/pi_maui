using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ProvaItem
    {
        public Prova Prova { get; set; } = new();
        public string Titulo => Prova.Titulo;
        public string MateriaNome => Prova.Materia?.Nome ?? string.Empty;
        public string Prazo => Prova.PrazoTexto;
        public string Duracao => Prova.DuracaoTexto;
        public string QuestoesTexto => $"{Prova.Questoes.Count} questões";
        public string ResumoTexto => $"{QuestoesTexto} · {Duracao}";
        public double NotaCorte => Prova.NotaCorte;
        public bool Expirada => Prova.Expirada;
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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiRealizadas), nameof(MostrarVazioRealizadas))]
        public partial ObservableCollection<ResultadoItem> Realizadas { get; private set; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarDisponiveis), nameof(MostrarRealizadas), nameof(MostrarVazioDisponiveis), nameof(MostrarVazioRealizadas))]
        public partial string AbaSelecionada { get; set; } = "disponiveis";

        public bool MostrarDisponiveis => AbaSelecionada == "disponiveis";
        public bool MostrarRealizadas => AbaSelecionada == "realizadas";
        public bool PossuiDisponiveis => Disponiveis.Count > 0;
        public bool PossuiRealizadas => Realizadas.Count > 0;
        public bool MostrarVazioDisponiveis => MostrarDisponiveis && !PossuiDisponiveis;
        public bool MostrarVazioRealizadas => MostrarRealizadas && !PossuiRealizadas;

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
            var provas = await ProvaService.VisiveisParaAlunoAsync(aluno.Id, matriculas);
            var resultados = await ResultadoRepositorio.DoAlunoAsync(aluno.Id);
            var respondidas = resultados.Select(r => r.ProvaId).ToHashSet();

            // provas já feitas que não aparecem mais para o aluno (ex.: despublicadas) são buscadas de uma vez
            var porId = provas.ToDictionary(p => p.Id);
            foreach (var prova in await ProvaRepositorio.PorIdsAsync(respondidas.Where(id => !porId.ContainsKey(id))))
                porId[prova.Id] = prova;

            Disponiveis = new(provas.Where(p => !respondidas.Contains(p.Id)).Select(p => new ProvaItem { Prova = p }));
            Realizadas = new(resultados.Select(r => ResultadoItem.De(r, porId.GetValueOrDefault(r.ProvaId))));
        });

        [RelayCommand]
        private void SelecionarAba(string aba) => AbaSelecionada = aba;

        [RelayCommand]
        private async Task IniciarAsync(ProvaItem? item)
        {
            if (item == null)
                return;

            if (item.Expirada)
            {
                await AvisarAsync("Prova encerrada", "O prazo desta prova já terminou.");
                return;
            }

            var tempo = item.Prova.DuracaoMinutos > 0
                ? $"Você terá {item.Prova.DuracaoMinutos} minutos. Ao final do tempo, a prova é enviada automaticamente."
                : "Esta prova não tem limite de tempo.";

            if (await ConfirmarAsync($"{item.Titulo}\n\n{item.QuestoesTexto}. {tempo}\nA prova só pode ser feita uma vez.", "Começar"))
                await Navegacao.IrAsync(Navegacao.ProvaRealizar, ("Prova", item.Prova));
        }
    }
}
