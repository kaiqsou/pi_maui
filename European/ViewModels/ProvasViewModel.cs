using System.Collections.ObjectModel;
using System.Windows.Input;
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

    public class ProvasViewModel : BaseViewModel
    {
        private string _abaSelecionada = "disponiveis";
        private ObservableCollection<ProvaItem> _disponiveis = new();
        private ObservableCollection<ResultadoItem> _realizadas = new();

        public ProvasViewModel()
        {
            Title = "Provas";
            SelecionarAbaCommand = new Command<string>(aba => AbaSelecionada = aba);
            IniciarCommand = new Command<ProvaItem>(async item => await IniciarAsync(item));
        }

        public ObservableCollection<ProvaItem> Disponiveis
        {
            get => _disponiveis;
            private set => SetProperty(ref _disponiveis, value);
        }

        public ObservableCollection<ResultadoItem> Realizadas
        {
            get => _realizadas;
            private set => SetProperty(ref _realizadas, value);
        }

        public string AbaSelecionada
        {
            get => _abaSelecionada;
            set
            {
                if (SetProperty(ref _abaSelecionada, value))
                {
                    OnPropertyChanged(nameof(MostrarDisponiveis));
                    OnPropertyChanged(nameof(MostrarRealizadas));
                    OnPropertyChanged(nameof(MostrarVazioDisponiveis));
                    OnPropertyChanged(nameof(MostrarVazioRealizadas));
                }
            }
        }

        public bool MostrarDisponiveis => AbaSelecionada == "disponiveis";
        public bool MostrarRealizadas => AbaSelecionada == "realizadas";

        public bool PossuiDisponiveis => Disponiveis.Count > 0;
        public bool PossuiRealizadas => Realizadas.Count > 0;
        public bool MostrarVazioDisponiveis => MostrarDisponiveis && !PossuiDisponiveis;
        public bool MostrarVazioRealizadas => MostrarRealizadas && !PossuiRealizadas;

        public ICommand SelecionarAbaCommand { get; }
        public ICommand IniciarCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            var disponiveis = new ObservableCollection<ProvaItem>();
            var realizadas = new ObservableCollection<ResultadoItem>();

            if (aluno != null)
            {
                var matriculas = await MatriculaRepositorio.AtivasDoAlunoAsync(aluno.Id);
                var provas = await ProvaService.VisiveisParaAlunoAsync(aluno.Id, matriculas);
                var resultados = await ResultadoRepositorio.DoAlunoAsync(aluno.Id);
                var respondidas = resultados.Select(r => r.ProvaId).ToHashSet();

                foreach (var prova in provas.Where(p => !respondidas.Contains(p.Id)))
                    disponiveis.Add(new ProvaItem { Prova = prova });

                foreach (var resultado in resultados)
                {
                    var prova = provas.FirstOrDefault(p => p.Id == resultado.ProvaId) ?? await ProvaRepositorio.PorIdAsync(resultado.ProvaId);
                    realizadas.Add(ResultadoItem.De(resultado, prova));
                }
            }

            Disponiveis = disponiveis;
            Realizadas = realizadas;

            OnPropertyChanged(nameof(PossuiDisponiveis));
            OnPropertyChanged(nameof(PossuiRealizadas));
            OnPropertyChanged(nameof(MostrarVazioDisponiveis));
            OnPropertyChanged(nameof(MostrarVazioRealizadas));
        });

        private static async Task IniciarAsync(ProvaItem? item)
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

            if (!await ConfirmarAsync($"{item.Titulo}\n\n{item.QuestoesTexto}. {tempo}\nA prova só pode ser feita uma vez.", "Começar"))
                return;

            await Shell.Current.GoToAsync("provarealizar", new Dictionary<string, object> { ["Prova"] = item.Prova });
        }
    }
}
