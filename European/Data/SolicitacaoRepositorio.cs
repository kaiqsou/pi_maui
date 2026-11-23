using European.Enums;
using European.Models;

namespace European.Data
{
    /// <summary>Pedidos de matrícula ainda não liberados (o candidato ainda não é usuário) e as fotos dos documentos.</summary>
    public static class SolicitacaoRepositorio
    {
        /// <summary>Solicitações em andamento (as canceladas ficam só para o candidato ver o reembolso).</summary>
        public static Task<List<SolicitacaoMatricula>> ListarAsync() => Api.GetAsync<List<SolicitacaoMatricula>>("solicitacoes");

        public static Task<SolicitacaoMatricula?> PorIdAsync(string id)
            => Api.GetOuNullAsync<SolicitacaoMatricula>($"solicitacoes/{Api.Esc(id)}");

        public static Task<SolicitacaoMatricula?> PorEmailAsync(string email)
            => Api.GetOuNullAsync<SolicitacaoMatricula>($"solicitacoes/email/{Api.Esc(UsuarioRepositorio.NormalizarEmail(email))}");

        /// <summary>
        /// Grava o pedido; retorna false se o e-mail já tem conta ou outro pedido em andamento.
        /// Um pedido cancelado do mesmo e-mail é apagado (pela API): o candidato pode se cadastrar de novo.
        /// </summary>
        public static Task<bool> InserirAsync(SolicitacaoMatricula solicitacao)
        {
            solicitacao.Id = Ids.Novo();
            solicitacao.Email = UsuarioRepositorio.NormalizarEmail(solicitacao.Email);
            solicitacao.Nome = solicitacao.Nome.Trim();
            solicitacao.CriadaEm = DateTime.Now;
            solicitacao.Status = StatusSolicitacao.Documentos;

            return Api.InserirAsync("solicitacoes", solicitacao);
        }

        /// <summary>Grava a solicitação inteira (status, documentos, pagamento, devolução...).</summary>
        public static Task AtualizarAsync(SolicitacaoMatricula solicitacao)
            => Api.PutAsync($"solicitacoes/{Api.Esc(solicitacao.Id)}", solicitacao);

        public static Task AtualizarSenhaAsync(string id, string senhaHash)
            => Api.PatchAsync($"solicitacoes/{Api.Esc(id)}/senha", new { SenhaHash = senhaHash });

        /// <summary>Apaga a solicitação e as fotos dos documentos (ao liberar, a conta do aluno já foi criada).</summary>
        public static Task ExcluirAsync(string id) => Api.DeleteAsync($"solicitacoes/{Api.Esc(id)}");

        // ---- Arquivos dos documentos (GridFS, pela API) ----

        /// <summary>Guarda o arquivo (foto ou PDF) e devolve o id.</summary>
        public static Task<string> EnviarArquivoAsync(string solicitacaoId, string tipo, string nomeArquivo, Stream conteudo)
            => Api.EnviarArquivoAsync(nomeArquivo, conteudo, new Dictionary<string, string>
            {
                ["solicitacaoId"] = solicitacaoId,
                ["documento"] = tipo
            });

        /// <summary>Baixa o arquivo para a pasta de cache (para exibir ou abrir) e devolve o caminho local.</summary>
        public static async Task<string> BaixarArquivoAsync(ArquivoDocumento arquivo)
        {
            var pasta = Path.Combine(FileSystem.CacheDirectory, "documentos", arquivo.ArquivoId);
            Directory.CreateDirectory(pasta);
            var caminho = Path.Combine(pasta, Path.GetFileName(string.IsNullOrEmpty(arquivo.NomeArquivo) ? "documento.jpg" : arquivo.NomeArquivo));

            if (!File.Exists(caminho))
            {
                try
                {
                    await using var destino = File.Create(caminho);
                    await Api.BaixarArquivoAsync(arquivo.ArquivoId, destino);
                }
                catch
                {
                    File.Delete(caminho); // não deixa um arquivo pela metade no cache
                    throw;
                }
            }

            return caminho;
        }

        public static Task ExcluirArquivosAsync(IEnumerable<ArquivoDocumento> arquivos)
        {
            var ids = arquivos.Select(a => a.ArquivoId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            return ids.Count == 0 ? Task.CompletedTask : Api.PostAsync("arquivos/excluir", new { ids });
        }
    }
}
