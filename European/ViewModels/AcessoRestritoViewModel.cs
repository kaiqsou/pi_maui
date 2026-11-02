using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Telas de quem ainda não acessa as aulas: candidato com solicitação aguardando a coordenação
    /// (ou com pendência), conta antiga pendente, ou ex-aluno (somente certificado).
    /// </summary>
    public class AcessoRestritoViewModel : BaseViewModel
    {
        private string _cursoNome = string.Empty;

        public AcessoRestritoViewModel()
        {
            VerificarCommand = new Command(async () => await VerificarAsync());
            SairCommand = new Command(async () => await AcessoService.SairAsync());
            RefazerPagamentoCommand = new Command(async () => await Shell.Current.GoToAsync(AcessoService.RotaPagamento));
        }

        public string Nome => SessionService.NomeAtual;
        public string Email => SessionService.EmailAtual;
        public string Iniciais => SessionService.IniciaisAtuais;

        public ICommand VerificarCommand { get; }
        public ICommand SairCommand { get; }
        public ICommand RefazerPagamentoCommand { get; }

        private static SolicitacaoMatricula? Solicitacao => SessionService.SolicitacaoAtual;

        /// <summary>Curso escolhido na solicitação de matrícula.</summary>
        public string CursoNome
        {
            get => _cursoNome;
            private set
            {
                if (SetProperty(ref _cursoNome, value))
                    OnPropertyChanged(nameof(TemPedido));
            }
        }

        public bool TemPedido => !string.IsNullOrEmpty(CursoNome);

        public string PagamentoTexto => Solicitacao is { PagamentoEm: { } quando, FormaPagamento: { } forma }
            ? $"Pagamento confirmado via {PagamentoService.Nome(forma)} em {quando:dd/MM/yyyy}"
            : string.Empty;

        /// <summary>Pendência registrada pela coordenação ao não liberar a matrícula.</summary>
        public bool TemPendencia => Solicitacao?.TemPendencia == true;
        public bool SemPendencia => !TemPendencia;
        public string PendenciaTexto => Solicitacao?.PendenciaMotivo ?? string.Empty;
        public bool PodeRefazerPagamento => TemPendencia;

        public void Atualizar()
        {
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Iniciais));
            OnPropertyChanged(nameof(PagamentoTexto));
            OnPropertyChanged(nameof(TemPendencia));
            OnPropertyChanged(nameof(SemPendencia));
            OnPropertyChanged(nameof(PendenciaTexto));
            OnPropertyChanged(nameof(PodeRefazerPagamento));
        }

        /// <summary>Relê a solicitação: a coordenação pode ter registrado uma pendência ou liberado a matrícula.</summary>
        public async Task CarregarPedidoAsync()
        {
            try
            {
                if (Solicitacao != null && await RecarregarSolicitacaoAsync() is { } rota)
                {
                    await Shell.Current.GoToAsync(rota);
                    return;
                }

                CursoNome = Solicitacao == null ? string.Empty : (await CursoRepositorio.PorIdAsync(Solicitacao.CursoId))?.Nome ?? string.Empty;
                Atualizar();
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(CarregarPedidoAsync), ex);
            }
        }

        /// <summary>
        /// Atualiza a solicitação da sessão. Se ela não existe mais, a coordenação liberou (virou usuário):
        /// entra com a conta nova e devolve a rota. Retorna null quando o candidato continua esperando.
        /// </summary>
        private static async Task<string?> RecarregarSolicitacaoAsync()
        {
            var atual = SessionService.SolicitacaoAtual!;
            var solicitacao = await SolicitacaoRepositorio.PorIdAsync(atual.Id);
            if (solicitacao != null)
            {
                SessionService.EntrarCandidato(solicitacao);
                return null;
            }

            var usuario = await UsuarioRepositorio.PorEmailAsync(atual.Email);
            if (usuario == null)
            {
                await AcessoService.SairAsync();
                return null;
            }

            var resultado = await AcessoService.RotaInicialAsync(usuario);
            SessionService.Entrar(usuario);
            return resultado.Rota;
        }

        private async Task VerificarAsync()
        {
            if (!SessionService.EstaLogado)
            {
                await AcessoService.SairAsync();
                return;
            }

            await ExecutarAsync(async () =>
            {
                // candidato: liberado quando a solicitação vira conta de usuário
                if (Solicitacao != null)
                {
                    var rota = await RecarregarSolicitacaoAsync();
                    if (rota != null)
                    {
                        await Shell.Current.GoToAsync(rota);
                        return;
                    }

                    Atualizar();
                    await AvisarAsync("Acesso", TemPendencia
                        ? "Sua matrícula tem uma pendência. Veja o motivo na tela."
                        : "Sua matrícula ainda não foi liberada pela coordenação.");
                    return;
                }

                // conta de usuário (pendente antiga ou ex-aluno)
                var atual = SessionService.UsuarioAtual!;
                var usuario = await UsuarioRepositorio.PorIdAsync(atual.Id) ?? atual;
                var resultado = await AcessoService.RotaInicialAsync(usuario);
                SessionService.Entrar(usuario);

                if (resultado.Rota == AcessoService.RotaAguardando)
                {
                    await AvisarAsync("Acesso", "Seu acesso ainda não foi liberado pela coordenação.");
                    return;
                }

                if (resultado.Rota != null)
                    await Shell.Current.GoToAsync(resultado.Rota);
            });
        }
    }
}
