using European.Models;

namespace European.Data
{
    public static class CertificadoRepositorio
    {
        // Versões antigas emitiam certificados por matéria (campo MateriaId); a API devolve só o certificado final do curso.

        public static Task<List<Certificado>> DoAlunoAsync(string alunoId)
            => Api.GetAsync<List<Certificado>>($"certificados/aluno/{Api.Esc(alunoId)}");

        public static Task<Certificado?> DaMatriculaAsync(string matriculaId)
            => Api.GetOuNullAsync<Certificado>($"certificados/matricula/{Api.Esc(matriculaId)}");

        /// <summary>Grava o certificado com um código de verificação único (derivado do Id, sem contador sujeito a repetição).</summary>
        public static Task InserirAsync(Certificado certificado)
        {
            certificado.Id = Ids.Novo();
            certificado.CodigoVerificacao = $"EURO-{certificado.EmitidoEm:yyyy}-{certificado.Id[^8..].ToUpperInvariant()}";
            return Api.PostAsync("certificados", certificado);
        }
    }
}
