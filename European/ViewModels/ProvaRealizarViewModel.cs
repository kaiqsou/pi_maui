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
        private string _texto = string.Empty;

        public int Numero { get; init; }
        public string Enunciado { get; init; } = string.Empty;
        public string PesoTexto { get; init; } = string.Empty;
        public bool IsDissertativa { get; init; }
        public bool IsAlternativas => !IsDissertativa;
        public List<AlternativaOpcao> Alternativas { get; } = new();

        private bool _atual;

        /// <summary>Resposta escrita (questão dissertativa).</summary>
        public string Texto
        {
            get => _texto;
            set
            {
                if (SetProperty(ref _texto, value))
                    OnPropertyChanged(nameof(Respondida));
            }
        }

        public bool EmBranco => IsDissertativa ? string.IsNullOrWhiteSpace(Texto) : Marcada < 0;
        public bool Respondida => !EmBranco;

        /// <summary>Questão aberta agora (destaca o número no mapa de questões).</summary>
        public bool Atual { get => _atual; set => SetProperty(ref _atual, value); }

        public int Marcada
        {
            get => _marcada;
            set
            {
                if (SetProperty(ref _marcada, value))
                {
                    foreach (var alternativa in Alternativas)
                        alternativa.Selecionada = alternativa.Indice == value;
                    OnPropertyChanged(nameof(Respondida));
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
            AnteriorCommand = new Command(() => IrPara(_indice - 1));
            ProximaCommand = new Command(async () =>
            {
                if (EhUltima)
                    await EnviarAsync(confirmar: true, tempoEsgotado: false);
                else
                    IrPara(_indice + 1);
            });
            IrParaCommand = new Command<QuestaoResposta>(q => { if (q != null) IrPara(Questoes.IndexOf(q)); });
        }

        public ObservableCollection<QuestaoResposta> Questoes { get; } = new();

        // ---- Uma questão por vez ----

        private int _indice;

        public QuestaoResposta? QuestaoAtual => _indice >= 0 && _indice < Questoes.Count ? Questoes[_indice] : null;
        public string IndicadorQuestao => Questoes.Count == 0 ? string.Empty : $"Questão {_indice + 1} de {Questoes.Count}";
        public bool PodeAnterior => _indice > 0;
        public bool EhUltima => _indice >= Questoes.Count - 1;
        public string BotaoProximaTexto => EhUltima ? "Enviar prova" : "Próxima";

        public ICommand AnteriorCommand { get; }
        public ICommand ProximaCommand { get; }
        public ICommand IrParaCommand { get; }

        private void IrPara(int indice)
        {
            if (Questoes.Count == 0)
                return;

            _indice = Math.Clamp(indice, 0, Questoes.Count - 1);
            for (var i = 0; i < Questoes.Count; i++)
                Questoes[i].Atual = i == _indice;

            OnPropertyChanged(nameof(QuestaoAtual));
            OnPropertyChanged(nameof(IndicadorQuestao));
            OnPropertyChanged(nameof(PodeAnterior));
            OnPropertyChanged(nameof(EhUltima));
            OnPropertyChanged(nameof(BotaoProximaTexto));
        }

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
                        PesoTexto = questao.Peso == 1 ? string.Empty : $"Peso {questao.Peso:0.#}",
                        IsDissertativa = questao.IsDissertativa
                    };

                    for (var j = 0; j < questao.Alternativas.Count; j++)
                        resposta.Alternativas.Add(new AlternativaOpcao { Questao = resposta, Indice = j, Texto = questao.Alternativas[j] });

                    Questoes.Add(resposta);
                }

                OnPropertyChanged(nameof(Instrucoes));
                OnPropertyChanged(nameof(TemInstrucoes));
                OnPropertyChanged(nameof(Subtitulo));
                OnPropertyChanged(nameof(TemTempo));
                IrPara(0);

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
                var emBranco = Questoes.Count(q => q.EmBranco);
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
                // a nota não é mostrada agora: a entrega vai para a correção do professor
                var resultado = ProvaService.Entregar(_prova, aluno.Id,
                    Questoes.Select(q => q.IsDissertativa ? -1 : q.Marcada).ToList(),
                    Questoes.Select(q => q.IsDissertativa ? q.Texto.Trim() : string.Empty).ToList(),
                    tempoEsgotado);
                await ResultadoRepositorio.InserirAsync(resultado);

                var titulo = tempoEsgotado ? "Tempo esgotado" : "Prova enviada";
                await AvisarAsync(titulo, "Suas respostas foram enviadas. A nota aparece em Provas › Realizadas depois que o professor corrigir.");
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
