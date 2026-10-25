using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class AlternativaOpcao : BaseViewModel
    {
        private bool _selecionada;

        public QuestaoResposta Questao { get; init; } = null!;
        public int Indice { get; init; }
        public string Letra => ((char)('A' + Indice)).ToString();
        public string Texto { get; init; } = string.Empty;

        public bool Selecionada
        {
            get => _selecionada;
            set => SetProperty(ref _selecionada, value);
        }
    }

    public class QuestaoResposta : BaseViewModel
    {
        private int _marcada = -1;

        public int Numero { get; init; }
        public string Enunciado { get; init; } = string.Empty;
        public string PesoTexto { get; init; } = string.Empty;
        public List<AlternativaOpcao> Alternativas { get; } = new();

        public int Marcada
        {
            get => _marcada;
            set
            {
                if (SetProperty(ref _marcada, value))
                {
                    foreach (var alternativa in Alternativas)
                        alternativa.Selecionada = alternativa.Indice == value;
                }
            }
        }
    }

    public class ProvaRealizarViewModel : BaseViewModel
    {
        private Prova? _prova;
        private IDispatcherTimer? _timer;
        private DateTime _fim;
        private string _tempoRestante = string.Empty;
        private bool _tempoAcabando;
        private bool _enviada;

        public ProvaRealizarViewModel()
        {
            SelecionarCommand = new Command<AlternativaOpcao>(a => { if (a != null && !_enviada) a.Questao.Marcada = a.Indice; });
            EnviarCommand = new Command(async () => await EnviarAsync(confirmar: true, tempoEsgotado: false));
            SairCommand = new Command(async () => await SairAsync());
        }

        public ObservableCollection<QuestaoResposta> Questoes { get; } = new();

        public string Instrucoes => _prova?.Instrucoes ?? string.Empty;
        public bool TemInstrucoes => !string.IsNullOrWhiteSpace(Instrucoes);
        public string Subtitulo => _prova == null ? string.Empty : $"{_prova.Questoes.Count} questões · nota máxima {_prova.NotaMaxima:0.#}";
        public bool TemTempo => _prova?.DuracaoMinutos > 0;

        public string TempoRestante
        {
            get => _tempoRestante;
            private set => SetProperty(ref _tempoRestante, value);
        }

        public bool TempoAcabando
        {
            get => _tempoAcabando;
            private set => SetProperty(ref _tempoAcabando, value);
        }

        public ICommand SelecionarCommand { get; }
        public ICommand EnviarCommand { get; }
        public ICommand SairCommand { get; }

        public async Task CarregarAsync(Prova prova)
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null)
                return;

            await ExecutarAsync(async () =>
            {
                if (await ResultadoRepositorio.ExisteAsync(aluno.Id, prova.Id))
                {
                    _enviada = true;
                    await AvisarAsync("Prova", "Você já realizou esta prova.");
                    await VoltarAsync();
                    return;
                }

                _prova = prova;
                Title = prova.Titulo;
                Questoes.Clear();

                for (var i = 0; i < prova.Questoes.Count; i++)
                {
                    var questao = prova.Questoes[i];
                    var resposta = new QuestaoResposta
                    {
                        Numero = i + 1,
                        Enunciado = questao.Enunciado,
                        PesoTexto = questao.Peso == 1 ? string.Empty : $"Peso {questao.Peso:0.#}"
                    };

                    for (var j = 0; j < questao.Alternativas.Count; j++)
                        resposta.Alternativas.Add(new AlternativaOpcao { Questao = resposta, Indice = j, Texto = questao.Alternativas[j] });

                    Questoes.Add(resposta);
                }

                OnPropertyChanged(nameof(Instrucoes));
                OnPropertyChanged(nameof(TemInstrucoes));
                OnPropertyChanged(nameof(Subtitulo));
                OnPropertyChanged(nameof(TemTempo));

                IniciarTimer(prova.DuracaoMinutos);
            });
        }

        private void IniciarTimer(int minutos)
        {
            if (minutos <= 0)
                return;

            _fim = DateTime.Now.AddMinutes(minutos);
            AtualizarTempo();

            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += async (_, _) =>
            {
                AtualizarTempo();
                if (DateTime.Now >= _fim)
                    await EnviarAsync(confirmar: false, tempoEsgotado: true);
            };
            _timer.Start();
        }

        private void AtualizarTempo()
        {
            var restante = _fim - DateTime.Now;
            if (restante < TimeSpan.Zero)
                restante = TimeSpan.Zero;

            TempoRestante = restante.ToString(restante.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss");
            TempoAcabando = restante.TotalMinutes < 1;
        }

        public void Parar() => _timer?.Stop();

        private async Task SairAsync()
        {
            if (_enviada || _prova == null)
            {
                await VoltarAsync();
                return;
            }

            if (await ConfirmarAsync("Se sair agora, a prova será enviada com as respostas marcadas até aqui.", "Enviar e sair"))
                await EnviarAsync(confirmar: false, tempoEsgotado: false);
        }

        private async Task EnviarAsync(bool confirmar, bool tempoEsgotado)
        {
            var aluno = SessionService.UsuarioAtual;
            if (_enviada || _prova == null || aluno == null)
                return;

            if (confirmar)
            {
                var emBranco = Questoes.Count(q => q.Marcada < 0);
                var mensagem = emBranco > 0
                    ? $"Ainda há {emBranco} questão(ões) sem resposta. Deseja enviar mesmo assim?"
                    : "Deseja enviar a prova? Não será possível alterar as respostas.";

                if (!await ConfirmarAsync(mensagem, "Enviar"))
                    return;
            }

            _enviada = true;
            Parar();

            var ok = await ExecutarAsync(async () =>
            {
                var resultado = ProvaService.Corrigir(_prova, aluno.Id, Questoes.Select(q => q.Marcada).ToList(), tempoEsgotado);
                await ResultadoRepositorio.InserirAsync(resultado);

                var matricula = (await MatriculaRepositorio.AtivasDoAlunoAsync(aluno.Id)).FirstOrDefault(m => m.CursoId == _prova.CursoId);
                if (matricula != null)
                    await ProgressoService.AtualizarAsync(matricula);

                var aprovado = resultado.NotaObtida >= _prova.NotaCorte;
                var titulo = tempoEsgotado ? "Tempo esgotado" : "Prova enviada";
                await AvisarAsync(titulo,
                    $"Nota: {resultado.NotaObtida:0.0} de {_prova.NotaMaxima:0.#}\n" +
                    $"{resultado.Acertos} de {resultado.TotalQuestoes} acertos\n" +
                    (aprovado ? "Aprovado nesta avaliação." : $"Abaixo da nota de corte ({_prova.NotaCorte:0.#})."));
            });

            if (!ok)
            {
                // permite tentar enviar de novo se o banco falhou
                _enviada = false;
                return;
            }

            await VoltarAsync();
        }
    }
}
