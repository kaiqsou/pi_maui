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
        /// <summary>Notificações (ex.-aluno ou matrícula trancada: avisos da coordenação continuam chegando).</summary>
        public Sino Sino { get; } = new();

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

        public bool Pago => Solicitacao?.Pago == true;

        public string PagamentoTexto => Solicitacao is not { Pago: true } s
            ? string.Empty
            : $"Pagamento confirmado {Formatos.Pagamento(s.FormaPagamento, s.PagamentoEm, "dd/MM/yyyy")}" +
              (s.TransacaoId is { } id ? $" · comprovante {id}" : string.Empty);

        /// <summary>Devolvida pela coordenação: o candidato corrige os documentos e reenvia.</summary>
        public bool TemPendencia => Solicitacao?.TemPendencia == true;
        public string PendenciaTexto => Solicitacao?.PendenciaMotivo ?? string.Empty;

        /// <summary>Cancelada (pela coordenação ou desistência), com o reembolso se houve pagamento.</summary>
        public bool Cancelada => Solicitacao?.Cancelada == true;
        public string CanceladaTexto => Solicitacao?.CanceladaMotivo ?? string.Empty;
        public string ReembolsoTexto => Solicitacao is { Reembolsada: true } s
            ? $"Reembolso de {Formatos.Moeda(s.ValorPago ?? 0)} feito em {s.ReembolsadaEm:dd/MM/yyyy} {ReembolsoComo(s)} (código {s.ReembolsoId})."
            : string.Empty;
        public bool TemReembolso => !string.IsNullOrEmpty(ReembolsoTexto);

        /// <summary>Aguardando a análise (nem devolvida nem cancelada), ou conta de usuário pendente/ex-aluno.</summary>
        public bool MostrarAguardando => !TemPendencia && !Cancelada;
        /// <summary>Desistência só depois que a solicitação chegou à análise (em análise ou devolvida para correção).</summary>
        public bool PodeDesistir => Solicitacao?.Status is StatusSolicitacao.EmAnalise or StatusSolicitacao.Devolvida;

        public string AguardandoTitulo => Solicitacao != null ? "Matrícula em análise" : "Aguardando liberação";
        public string AguardandoTexto => Solicitacao != null
            ? "Recebemos seus documentos e o pagamento. A coordenação está conferindo tudo; assim que liberar, as aulas e provas aparecerão aqui."
            : "Assim que a coordenação liberar o seu acesso ao curso, as aulas e provas aparecerão aqui.";

        private static string ReembolsoComo(SolicitacaoMatricula s) => s.FormaPagamento switch
        {
            FormaPagamento.Pix => "via Pix na conta de origem",
            FormaPagamento.CartaoCredito => "como estorno no cartão de crédito",
            FormaPagamento.CartaoDebito => "como estorno no cartão de débito",
            _ => string.Empty
        };

        /// <summary>Relê a solicitação: a coordenação pode ter registrado uma pendência ou liberado a matrícula.</summary>
        public override async Task AoAparecerAsync()
        {
            NotificarDadosDaSessao();
            await Sino.AtualizarAsync();
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
            OnPropertyChanged(nameof(Pago));
            OnPropertyChanged(nameof(PagamentoTexto));
            OnPropertyChanged(nameof(TemPendencia));
            OnPropertyChanged(nameof(PendenciaTexto));
            OnPropertyChanged(nameof(Cancelada));
            OnPropertyChanged(nameof(CanceladaTexto));
            OnPropertyChanged(nameof(ReembolsoTexto));
            OnPropertyChanged(nameof(TemReembolso));
            OnPropertyChanged(nameof(MostrarAguardando));
            OnPropertyChanged(nameof(PodeDesistir));
            OnPropertyChanged(nameof(AguardandoTitulo));
            OnPropertyChanged(nameof(AguardandoTexto));
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
                // ainda enviando documentos ou pagando: a etapa certa é outra tela
                SessionService.EntrarCandidato(solicitacao);
                var etapa = AcessoService.RotaCandidato(solicitacao);
                return etapa == Navegacao.Aguardando ? null : etapa;
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

        /// <summary>Devolvida: abre os documentos para trocar os recusados e reenviar.</summary>
        [RelayCommand]
        private Task CorrigirDocumentos() => Shell.Current.GoToAsync(Navegacao.Documentos);

        [RelayCommand]
        private async Task DesistirAsync()
        {
            if (Solicitacao is { } solicitacao && await Candidatura.DesistirAsync(solicitacao))
                NotificarDadosDaSessao();
        }

        /// <summary>Cancelada: começa um cadastro novo (o pedido cancelado é substituído).</summary>
        [RelayCommand]
        private async Task NovoCadastroAsync()
        {
            SessionService.Sair();
            await Shell.Current.GoToAsync(Navegacao.Login);
            await Navegacao.IrAsync(Navegacao.Cadastro);
        }

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
                    await AvisarAsync("Acesso", Cancelada ? "Sua solicitação foi cancelada."
                        : TemPendencia ? "A coordenação pediu correções. Veja o motivo na tela."
                        : "Sua matrícula ainda está em análise.");
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
