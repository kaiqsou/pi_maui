using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>
    /// Etapas da solicitação de matrícula: o candidato envia os documentos e paga; a coordenação analisa e libera,
    /// devolve para correção ou cancela (com reembolso). O candidato também pode desistir antes de a conta ser criada.
    /// </summary>
    public static class SolicitacaoService
    {
        /// <summary>Tamanho máximo de cada arquivo de documento (foto ou PDF).</summary>
        public const long TamanhoMaximoArquivo = 10 * 1024 * 1024;

        /// <summary>
        /// Acrescenta um arquivo (foto ou PDF) ao documento — ex.: o verso depois da frente. Mexer no documento tira a
        /// aprovação ou a recusa: a coordenação confere de novo.
        /// </summary>
        public static async Task AdicionarArquivoAsync(SolicitacaoMatricula solicitacao, string tipo, string nomeArquivo, Stream conteudo)
        {
            var documento = solicitacao.Documento(tipo);
            if (documento == null)
            {
                documento = new DocumentoEnviado { Tipo = tipo };
                solicitacao.Documentos.Add(documento);
            }

            if (documento.Arquivos.Count >= DocumentoEnviado.MaximoArquivos)
                throw new InvalidOperationException($"Envie no máximo {DocumentoEnviado.MaximoArquivos} arquivos por documento.");

            var arquivoId = await SolicitacaoRepositorio.EnviarArquivoAsync(solicitacao.Id, tipo, nomeArquivo, conteudo);
            documento.Arquivos.Add(new ArquivoDocumento { ArquivoId = arquivoId, NomeArquivo = nomeArquivo });
            ReabrirAnalise(documento);
            await SolicitacaoRepositorio.AtualizarAsync(solicitacao);
        }

        /// <summary>Tira um arquivo do documento (e apaga do banco).</summary>
        public static async Task RemoverArquivoAsync(SolicitacaoMatricula solicitacao, string tipo, string arquivoId)
        {
            if (solicitacao.Documento(tipo) is not { } documento || documento.Arquivos.FirstOrDefault(a => a.ArquivoId == arquivoId) is not { } arquivo)
                return;

            documento.Arquivos.Remove(arquivo);
            ReabrirAnalise(documento);
            if (!documento.Enviado)
                solicitacao.Documentos.Remove(documento);
            await SolicitacaoRepositorio.AtualizarAsync(solicitacao);
            await SolicitacaoRepositorio.ExcluirArquivosAsync(new[] { arquivo });
        }

        private static void ReabrirAnalise(DocumentoEnviado documento)
        {
            documento.Aprovado = false;
            documento.Recusado = false;
            documento.Observacao = null;
        }

        /// <summary>
        /// Candidato terminou de enviar os documentos: segue para o pagamento ou, se já pagou (correção de uma
        /// devolução), volta para a análise da coordenação. Retorna o erro, ou null se avançou.
        /// </summary>
        public static async Task<string?> ConcluirDocumentosAsync(SolicitacaoMatricula solicitacao)
        {
            if (!solicitacao.DocumentosCompletos)
                return "Envie todos os documentos (e corrija os recusados) para continuar.";

            if (!solicitacao.Pago)
            {
                solicitacao.Status = StatusSolicitacao.Pagamento;
                await SolicitacaoRepositorio.AtualizarAsync(solicitacao);
                return null;
            }

            var eraDevolucao = solicitacao.Status == StatusSolicitacao.Devolvida;
            solicitacao.Status = StatusSolicitacao.EmAnalise;
            solicitacao.PendenciaMotivo = null;
            solicitacao.PendenciaEm = null;
            await SolicitacaoRepositorio.AtualizarAsync(solicitacao);

            await NotificacaoService.ParaCoordenacaoAsync(
                eraDevolucao ? "Solicitação corrigida" : "Solicitação de matrícula",
                eraDevolucao
                    ? $"{solicitacao.Nome} reenviou os documentos e aguarda nova análise."
                    : $"{solicitacao.Nome} enviou os documentos e o pagamento e aguarda a análise.");
            return null;
        }

        /// <summary>Registra o pagamento da matrícula e manda a solicitação para a análise da coordenação.</summary>
        public static async Task RegistrarPagamentoAsync(SolicitacaoMatricula solicitacao, FormaPagamento forma, int parcelas, decimal valor)
        {
            solicitacao.FormaPagamento = forma;
            solicitacao.Parcelas = forma == FormaPagamento.CartaoCredito ? Math.Max(1, parcelas) : 1;
            solicitacao.PagamentoEm = DateTime.Now;
            solicitacao.ValorPago = valor;
            solicitacao.TransacaoId = NovoCodigo("PAG");
            solicitacao.Status = solicitacao.DocumentosCompletos ? StatusSolicitacao.EmAnalise : StatusSolicitacao.Documentos;
            await SolicitacaoRepositorio.AtualizarAsync(solicitacao);

            if (solicitacao.Status == StatusSolicitacao.EmAnalise)
                await NotificacaoService.ParaCoordenacaoAsync("Solicitação de matrícula",
                    $"{solicitacao.Nome} enviou os documentos e pagou {Formatos.Moeda(valor)}. A solicitação aguarda a análise.");
        }

        /// <summary>Código de transação/reembolso, ex.: "PAG-20261120-7K3QX9".</summary>
        private static string NovoCodigo(string prefixo)
        {
            const string letras = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var sufixo = new string(Enumerable.Range(0, 6).Select(_ => letras[System.Security.Cryptography.RandomNumberGenerator.GetInt32(letras.Length)]).ToArray());
            return $"{prefixo}-{DateTime.Now:yyyyMMdd}-{sufixo}";
        }

        /// <summary>Coordenação aprova o documento, ou recusa com a observação para o candidato.</summary>
        public static Task AvaliarDocumentoAsync(SolicitacaoMatricula solicitacao, string tipo, bool aprovado, string? observacao)
        {
            if (solicitacao.Documento(tipo) is not { } documento)
                return Task.CompletedTask;

            documento.Aprovado = aprovado;
            documento.Recusado = !aprovado;
            documento.Observacao = aprovado ? null : observacao?.Trim();
            return SolicitacaoRepositorio.AtualizarAsync(solicitacao);
        }

        /// <summary>Coordenação devolve ao candidato para corrigir (ele vê o motivo e os documentos recusados ao entrar).</summary>
        public static Task DevolverAsync(SolicitacaoMatricula solicitacao, string motivo)
        {
            solicitacao.Status = StatusSolicitacao.Devolvida;
            solicitacao.PendenciaMotivo = motivo.Trim();
            solicitacao.PendenciaEm = DateTime.Now;
            return SolicitacaoRepositorio.AtualizarAsync(solicitacao);
        }

        /// <summary>
        /// Cancela a solicitação (pela coordenação, ou desistência do candidato) e reembolsa o pagamento, se houve.
        /// As fotos dos documentos são apagadas; o registro fica para o candidato ver o cancelamento e o reembolso,
        /// e é substituído se ele se cadastrar de novo.
        /// </summary>
        public static async Task CancelarAsync(SolicitacaoMatricula solicitacao, string motivo, bool peloCandidato)
        {
            if (solicitacao.Cancelada)
                return;

            if (solicitacao.Pago && solicitacao.ReembolsoId == null)
            {
                solicitacao.ReembolsoId = NovoCodigo("EST");
                solicitacao.ReembolsadaEm = DateTime.Now;
            }

            await SolicitacaoRepositorio.ExcluirArquivosAsync(solicitacao.Documentos.SelectMany(d => d.Arquivos));
            solicitacao.Documentos.Clear();
            solicitacao.Status = StatusSolicitacao.Cancelada;
            solicitacao.CanceladaEm = DateTime.Now;
            solicitacao.CanceladaMotivo = motivo.Trim();
            await SolicitacaoRepositorio.AtualizarAsync(solicitacao);

            if (peloCandidato)
                await NotificacaoService.ParaCoordenacaoAsync("Desistência de matrícula",
                    $"{solicitacao.Nome} desistiu da matrícula{(solicitacao.ReembolsoId != null ? " e o pagamento foi reembolsado" : string.Empty)}.");
        }

        /// <summary>Texto da etapa, para a coordenação e para o candidato.</summary>
        public static string StatusTexto(SolicitacaoMatricula solicitacao) => solicitacao.Status switch
        {
            StatusSolicitacao.Documentos => "Aguardando documentos",
            StatusSolicitacao.Pagamento => "Aguardando pagamento",
            StatusSolicitacao.EmAnalise => "Aguardando análise",
            StatusSolicitacao.Devolvida => "Devolvida para correção",
            _ => solicitacao.Reembolsada ? "Cancelada e reembolsada" : "Cancelada"
        };
    }
}
