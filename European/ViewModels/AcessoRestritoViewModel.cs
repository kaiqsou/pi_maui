using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Telas de quem ainda não acessa as aulas: candidato com solicitação aguardando a coordenação
    /// (ou com pendência), conta antiga pendente, ou ex-aluno (somente certificado).
    /// </summary>
    public partial class AcessoRestritoViewModel : BaseViewModel
    {
        public string Nome => SessionService.NomeAtual;
        public string Email => SessionService.EmailAtual;
        public string Iniciais => SessionService.IniciaisAtuais;

        private static SolicitacaoMatricula? Solicitacao => SessionService.SolicitacaoAtual;

        /// <summary>Curso escolhido na solicitação de matrícula.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemPedido))]
        public partial string CursoNome { get; private set; } = string.Empty;

        public bool TemPedido => !string.IsNullOrEmpty(CursoNome);

        /// <summary>Ex-aluno que trancou a matrícula: pode reabrir por aqui.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMatriculaTrancada), nameof(AvisoTexto))]
        private partial Matricula? MatriculaTrancada { get; set; }

        public bool TemMatriculaTrancada => MatriculaTrancada != null;

        public string AvisoTexto => MatriculaTrancada is { } trancada
            ? $"Sua matrícula em {trancada.CursoNome} está trancada{(trancada.TrancadaEm is { } em ? $" desde {em:dd/MM/yyyy}" : string.Empty)}. Reabra para voltar às aulas e provas."
            : "Seu acesso às aulas foi encerrado. Você continua podendo consultar o seu certificado.";

        public string PagamentoTexto => Solicitacao == null || !Solicitacao.Pago
            ? string.Empty
            : $"Pagamento confirmado {Formatos.Pagamento(Solicitacao.FormaPagamento, Solicitacao.PagamentoEm, "dd/MM/yyyy")}";

        /// <summary>Pendência registrada pela coordenação ao não liberar a matrícula.</summary>
        public bool TemPendencia => Solicitacao?.TemPendencia == true;
        public bool SemPendencia => !TemPendencia;
        public string PendenciaTexto => Solicitacao?.PendenciaMotivo ?? string.Empty;
        public bool PodeRefazerPagamento => TemPendencia;

        /// <summary>Relê a solicitação: a coordenação pode ter registrado uma pendência ou liberado a matrícula.</summary>
        public override async Task AoAparecerAsync()
        {
            NotificarDadosDaSessao();
            try
            {
                if (Solicitacao != null && await RecarregarSolicitacaoAsync() is { } rota)
                {
                    await Shell.Current.GoToAsync(rota);
                    return;
                }

                CursoNome = Solicitacao == null ? string.Empty : (await CursoRepositorio.PorIdAsync(Solicitacao.CursoId))?.Nome ?? string.Empty;
                MatriculaTrancada = SessionService.UsuarioAtual is { } usuario
                    ? (await MatriculaRepositorio.DoAlunoAsync(usuario.Id)).FirstOrDefault(m => m.Status == StatusMatricula.Trancada)
                    : null;
                NotificarDadosDaSessao();
            }
            catch (Exception ex)
            {
                // sem conexão: a tela continua com os dados que já tinha
                CrashLogger.Registrar(nameof(AcessoRestritoViewModel), ex);
            }
        }

        private void NotificarDadosDaSessao()
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

        [RelayCommand]
        private Task Sair() => AcessoService.SairAsync();

        [RelayCommand]
        private async Task ReabrirMatriculaAsync()
        {
            var usuario = SessionService.UsuarioAtual;
            if (usuario == null || MatriculaTrancada == null)
                return;

            if (await ExecutarAsync(() => ContaService.ReabrirMatriculaAsync(usuario, MatriculaTrancada)))
                await AcessoService.EntrarAsync(usuario);
        }

        [RelayCommand]
        private Task RefazerPagamento() => Shell.Current.GoToAsync(Navegacao.Pagamento);

        [RelayCommand]
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

                    NotificarDadosDaSessao();
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

                if (resultado.Rota == Navegacao.Aguardando)
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
