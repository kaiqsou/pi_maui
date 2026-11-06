using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>O aluno faz a prova: uma questão por vez, com cronômetro quando a prova tem tempo limite.</summary>
    public partial class ProvaRealizarViewModel : BaseViewModel
    {
        private Prova? _prova;
        private IDispatcherTimer? _timer;
        private DateTime _fim;
        private bool _enviada;
        private int _indice;

        public ObservableCollection<QuestaoResposta> Questoes { get; } = new();

        public QuestaoResposta? QuestaoAtual => _indice >= 0 && _indice < Questoes.Count ? Questoes[_indice] : null;
        public string IndicadorQuestao => Questoes.Count == 0 ? string.Empty : $"Questão {_indice + 1} de {Questoes.Count}";
        public bool PodeAnterior => _indice > 0;
        public bool EhUltima => _indice >= Questoes.Count - 1;
        public string BotaoProximaTexto => EhUltima ? "Enviar prova" : "Próxima";

        public string Instrucoes => _prova?.Instrucoes ?? string.Empty;
        public bool TemInstrucoes => !string.IsNullOrWhiteSpace(Instrucoes);
        public string Subtitulo => _prova == null ? string.Empty : $"{_prova.Questoes.Count} questões · nota máxima {_prova.NotaMaxima:0.#}";
        public bool TemTempo => _prova?.DuracaoMinutos > 0;

        [ObservableProperty]
        public partial string TempoRestante { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial bool TempoAcabando { get; private set; }

        protected override async Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null || parametros.Obter<Prova>("Prova") is not { } prova)
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
                    Questoes.Add(QuestaoResposta.De(prova.Questoes[i], i));

                OnPropertyChanged(nameof(Instrucoes));
                OnPropertyChanged(nameof(TemInstrucoes));
                OnPropertyChanged(nameof(Subtitulo));
                OnPropertyChanged(nameof(TemTempo));
                MostrarQuestao(0);

                IniciarTimer(prova.DuracaoMinutos);
            });
        }

        // ---- Navegação entre questões ----

        [RelayCommand]
        private void Anterior() => MostrarQuestao(_indice - 1);

        [RelayCommand]
        private async Task ProximaAsync()
        {
            if (EhUltima)
                await EnviarAsync(confirmar: true, tempoEsgotado: false);
            else
                MostrarQuestao(_indice + 1);
        }

        [RelayCommand]
        private void IrPara(QuestaoResposta? questao)
        {
            if (questao != null)
                MostrarQuestao(Questoes.IndexOf(questao));
        }

        [RelayCommand]
        private void Selecionar(AlternativaOpcao? alternativa)
        {
            if (alternativa != null && !_enviada)
                alternativa.Questao.Marcada = alternativa.Indice;
        }

        private void MostrarQuestao(int indice)
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

        // ---- Cronômetro ----

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

            TempoRestante = Formatos.Tempo(restante.TotalSeconds);
            TempoAcabando = restante.TotalMinutes < 1;
        }

        /// <summary>Para o cronômetro quando a tela some.</summary>
        public void Parar() => _timer?.Stop();

        // ---- Envio ----

        /// <summary>Sair (botão ou voltar do sistema) envia a prova com o que já foi respondido.</summary>
        [RelayCommand]
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
