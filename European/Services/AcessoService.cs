using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>Decide para qual área do app cada usuário vai, conforme o papel e o acesso liberado pela coordenação.</summary>
    public static class AcessoService
    {

        public sealed record Resultado(string? Rota, string? Mensagem = null);

        public static async Task<Resultado> RotaInicialAsync(Usuario usuario)
        {
            if (usuario.IsCoordenacao)
            {
                return usuario.AcessoLiberado
                    ? new Resultado(Navegacao.Coordenacao)
                    : new Resultado(null, "Seu acesso à coordenação está desativado.");
            }

            if (usuario.Roles == Roles.Professor)
            {
                return usuario.AcessoLiberado
                    ? new Resultado(Navegacao.Professor)
                    : new Resultado(null, "Seu acesso de professor foi removido pela coordenação.");
            }

            var matriculas = await MatriculaRepositorio.DoAlunoAsync(usuario.Id);

            // Após concluir o curso, o aluno ainda reassiste as aulas até o fim da validade do acesso.
            if (usuario.AcessoLiberado && matriculas.Any(m => m.AcessoAulasVigente))
                return new Resultado(Navegacao.Aluno);

            // Quem já foi aluno (matrícula encerrada, ou concluída com a validade esgotada) só consulta o próprio certificado.
            if (matriculas.Count > 0 || (await CertificadoRepositorio.DoAlunoAsync(usuario.Id)).Count > 0)
                return new Resultado(Navegacao.SomenteCertificados);

            return new Resultado(Navegacao.Aguardando);
        }

        /// <summary>Candidato (solicitação ainda não liberada): paga primeiro, depois aguarda a coordenação.</summary>
        public static string RotaCandidato(SolicitacaoMatricula solicitacao)
            => solicitacao.Pago ? Navegacao.Aguardando : Navegacao.Pagamento;

        public static async Task EntrarAsync(Usuario usuario)
        {
            var resultado = await RotaInicialAsync(usuario);
            if (resultado.Rota == null)
            {
                await Shell.Current.DisplayAlertAsync("Acesso", resultado.Mensagem, "OK");
                return;
            }

            SessionService.Entrar(usuario);

            // conta criada (ou senha redefinida) pela coordenação: primeiro troca a senha provisória
            await Shell.Current.GoToAsync(usuario.SenhaProvisoria ? Navegacao.NovaSenha : resultado.Rota);
        }

        public static async Task EntrarCandidatoAsync(SolicitacaoMatricula solicitacao)
        {
            SessionService.EntrarCandidato(solicitacao);
            await Shell.Current.GoToAsync(RotaCandidato(solicitacao));
        }

        public static async Task SairAsync()
        {
            SessionService.Sair();
            await Shell.Current.GoToAsync(Navegacao.Login);
        }
    }
}
