using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Ações do candidato (solicitação ainda não liberada) comuns às telas de documentos, pagamento e espera.</summary>
    public static class Candidatura
    {
        /// <summary>Relê a solicitação do banco e atualiza a sessão (a coordenação pode ter mudado algo).</summary>
        public static async Task<SolicitacaoMatricula?> RecarregarAsync()
        {
            if (SessionService.SolicitacaoAtual is not { } atual)
                return null;

            var solicitacao = await SolicitacaoRepositorio.PorIdAsync(atual.Id);
            if (solicitacao != null)
                SessionService.EntrarCandidato(solicitacao);
            return solicitacao;
        }

        /// <summary>Vai para a tela da etapa atual (documentos, pagamento ou espera).</summary>
        public static Task IrParaEtapaAsync(SolicitacaoMatricula solicitacao)
        {
            SessionService.EntrarCandidato(solicitacao);
            return Shell.Current.GoToAsync(AcessoService.RotaCandidato(solicitacao));
        }

        /// <summary>
        /// O candidato desiste antes de a conta ser criada: a solicitação é cancelada e, se ele já pagou, o valor é
        /// reembolsado. Retorna true se cancelou.
        /// </summary>
        public static async Task<bool> DesistirAsync(SolicitacaoMatricula solicitacao)
        {
            var pergunta = solicitacao.Pago
                ? "Deseja desistir da matrícula? O valor pago será reembolsado."
                : "Deseja desistir da matrícula?";
            if (!await Shell.Current.DisplayAlertAsync("Desistir", pergunta, "Desistir", "Voltar"))
                return false;

            try
            {
                await SolicitacaoService.CancelarAsync(solicitacao, "Desistência do candidato", peloCandidato: true);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(Candidatura), ex);
                await Shell.Current.DisplayAlertAsync("Desistir", "Não foi possível cancelar agora. Tente novamente.", "OK");
                return false;
            }

            await IrParaEtapaAsync(solicitacao);
            return true;
        }
    }
}
