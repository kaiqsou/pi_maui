using European.Models;

namespace European.Data
{
    /// <summary>Materiais complementares das aulas. Arquivos enviados ficam no GridFS, através da API.</summary>
    public static class MaterialRepositorio
    {
        public static Task<List<MaterialComplementar>> DaAulaAsync(string aulaId)
            => Api.GetAsync<List<MaterialComplementar>>($"materiais/aula/{Api.Esc(aulaId)}");

        /// <summary>Envia o arquivo (se houver) e grava o material no fim da lista da aula.</summary>
        public static async Task InserirAsync(MaterialComplementar material, Stream? arquivo = null)
        {
            if (arquivo != null && !string.IsNullOrEmpty(material.NomeArquivo))
            {
                material.ArquivoId = await Api.EnviarArquivoAsync(material.NomeArquivo, arquivo, new Dictionary<string, string>
                {
                    ["aulaId"] = material.AulaId,
                    ["cursoId"] = material.CursoId
                });
            }

            var gravado = await Api.PostAsync<MaterialComplementar>("materiais", material);
            material.Id = gravado.Id;
            material.Ordem = gravado.Ordem;
        }

        /// <summary>Exclui o material e o arquivo dele.</summary>
        public static Task ExcluirAsync(MaterialComplementar material) => Api.DeleteAsync($"materiais/{Api.Esc(material.Id)}");

        public static Task ExcluirDasAulasAsync(IEnumerable<string> aulaIds)
        {
            var ids = aulaIds.Distinct().ToList();
            return ids.Count == 0 ? Task.CompletedTask : Api.PostAsync("materiais/excluir-aulas", new { ids });
        }

        public static Task ExcluirDoCursoAsync(string cursoId) => Api.DeleteAsync($"materiais/curso/{Api.Esc(cursoId)}");

        /// <summary>Baixa o arquivo para a pasta de cache e devolve o caminho local.</summary>
        public static async Task<string> BaixarAsync(MaterialComplementar material)
        {
            if (string.IsNullOrEmpty(material.ArquivoId))
                throw new InvalidOperationException("O material não possui arquivo.");

            var pasta = Path.Combine(FileSystem.CacheDirectory, "materiais", material.Id);
            Directory.CreateDirectory(pasta);

            var caminho = Path.Combine(pasta, Path.GetFileName(material.NomeArquivo ?? "material"));
            await using (var destino = File.Create(caminho))
                await Api.BaixarArquivoAsync(material.ArquivoId, destino);

            return caminho;
        }
    }
}
