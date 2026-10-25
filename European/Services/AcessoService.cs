using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>Decide para qual área do app cada usuário vai, conforme o papel e o acesso liberado pela coordenação.</summary>
    public static class AcessoService
    {
        public const string RotaCoordenacao = "//coordenacao";
        public const string RotaProfessor = "//professor";
        public const string RotaAluno = "//main";
        public const string RotaSomenteCertificados = "//certificados";
        public const string RotaAguardando = "//aguardando";

        public sealed record Resultado(string? Rota, string? Mensagem = null);

        public static async Task<Resultado> RotaInicialAsync(Usuario usuario)
        {
            if (usuario.IsCoordenacao)
            {
                return usuario.AcessoLiberado
                    ? new Resultado(RotaCoordenacao)
                    : new Resultado(null, "Seu acesso à coordenação está desativado.");
            }

            if (usuario.Roles == Roles.Professor)
            {
                return usuario.AcessoLiberado
                    ? new Resultado(RotaProfessor)
                    : new Resultado(null, "Seu acesso de professor foi removido pela coordenação.");
            }

            var matriculas = await MatriculaRepositorio.DoAlunoAsync(usuario.Id);

            if (usuario.AcessoLiberado && matriculas.Any(m => m.Status == StatusMatricula.Ativa))
                return new Resultado(RotaAluno);

            // Quem já foi aluno (matrícula encerrada ou certificado emitido) só consulta o próprio certificado.
            if (matriculas.Count > 0 || (await CertificadoRepositorio.DoAlunoAsync(usuario.Id)).Count > 0)
                return new Resultado(RotaSomenteCertificados);

            return new Resultado(RotaAguardando);
        }

        public static async Task EntrarAsync(Usuario usuario)
        {
            var resultado = await RotaInicialAsync(usuario);
            if (resultado.Rota == null)
            {
                await Shell.Current.DisplayAlert("Acesso", resultado.Mensagem, "OK");
                return;
            }

            SessionService.Entrar(usuario);
            await Shell.Current.GoToAsync(resultado.Rota);
        }

        public static async Task SairAsync()
        {
            SessionService.Sair();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
