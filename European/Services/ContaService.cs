using System.Net.Mail;
using System.Security.Cryptography;
using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>Contas e acesso de alunos e professores: criação, liberação de curso e remoção de acesso.</summary>
    public static class ContaService
    {
        public static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email.Trim()).Address == email.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>Confere os dados de uma conta nova (nome, e-mail e senha). Retorna o erro, ou null se estiverem ok.</summary>
        public static string? ValidarDados(string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
                return "Informe nome, e-mail e senha.";
            if (!EmailValido(email))
                return "Informe um e-mail válido.";
            return SenhaService.EhValida(senha, out var erro) ? null : erro;
        }

        /// <summary>Login conferido com um hash antigo (SHA-256 sem salt): grava a senha no formato atual.</summary>
        public static async Task AtualizarHashAntigoAsync(Usuario usuario, string senha)
        {
            if (!SenhaService.PrecisaAtualizar(usuario.SenhaHash))
                return;

            usuario.SenhaHash = SenhaService.GerarHash(senha);
            await UsuarioRepositorio.AtualizarSenhaAsync(usuario.Id, usuario.SenhaHash, usuario.SenhaProvisoria);
        }

        public static async Task AtualizarHashAntigoAsync(SolicitacaoMatricula solicitacao, string senha)
        {
            if (!SenhaService.PrecisaAtualizar(solicitacao.SenhaHash))
                return;

            solicitacao.SenhaHash = SenhaService.GerarHash(senha);
            await SolicitacaoRepositorio.AtualizarSenhaAsync(solicitacao.Id, solicitacao.SenhaHash);
        }

        /// <summary>Senha provisória fácil de ditar: 5 letras e 3 números, sem caracteres ambíguos (l, o, 0, 1).</summary>
        public static string GerarSenhaProvisoria()
        {
            const string letras = "abcdefghjkmnpqrstuvwxyz";
            const string numeros = "23456789";
            var caracteres = Enumerable.Range(0, 5).Select(_ => letras[RandomNumberGenerator.GetInt32(letras.Length)])
                .Concat(Enumerable.Range(0, 3).Select(_ => numeros[RandomNumberGenerator.GetInt32(numeros.Length)]));
            return new string(caracteres.ToArray());
        }

        /// <summary>Cria o usuário e, se houver curso, a matrícula ativa. Retorna false se o e-mail já tem conta.</summary>
        public static async Task<bool> CriarContaAsync(Usuario usuario, string? cursoId)
        {
            if (!await UsuarioRepositorio.InserirAsync(usuario))
                return false;

            if (cursoId != null)
                await MatriculaRepositorio.InserirAsync(NovaMatricula(usuario.Id, cursoId));
            return true;
        }

        /// <summary>
        /// Libera uma solicitação de matrícula: cria a conta do aluno com os dados do pedido (só aqui o e-mail passa a
        /// existir como usuário), a matrícula, e apaga a solicitação. Retorna false se o e-mail já tem conta.
        /// </summary>
        public static async Task<bool> LiberarSolicitacaoAsync(SolicitacaoMatricula solicitacao)
        {
            var aluno = new Usuario
            {
                Nome = solicitacao.Nome,
                Email = solicitacao.Email,
                SenhaHash = solicitacao.SenhaHash,
                Roles = Roles.Aluno,
                StatusAcesso = StatusAcesso.Liberado,
                FormaPagamento = solicitacao.FormaPagamento,
                PagamentoEm = solicitacao.PagamentoEm
            };

            if (!await CriarContaAsync(aluno, solicitacao.CursoId))
                return false;

            await SolicitacaoRepositorio.ExcluirAsync(solicitacao.Id);
            await NotificacaoService.ParaUsuarioAsync(aluno.Id, "Bem-vindo(a)!", "Sua matrícula foi liberada. As aulas já estão disponíveis.");
            return true;
        }

        /// <summary>
        /// O próprio aluno tranca a matrícula ativa: deixa de acessar aulas e provas até reabrir (o progresso fica guardado).
        /// A coordenação é avisada, com o motivo se ele informar.
        /// </summary>
        public static async Task TrancarMatriculaAsync(Usuario aluno, Matricula matricula, string? motivo)
        {
            if (matricula.Status != StatusMatricula.Ativa)
                throw new InvalidOperationException("Só uma matrícula ativa pode ser trancada.");

            matricula.Status = StatusMatricula.Trancada;
            matricula.TrancadaEm = DateTime.Now;
            await MatriculaRepositorio.AtualizarAsync(matricula);

            var detalhe = string.IsNullOrWhiteSpace(motivo) ? string.Empty : $" Motivo: {motivo.Trim()}";
            await NotificacaoService.ParaCoordenacaoAsync("Matrícula trancada",
                $"{aluno.Nome} trancou a matrícula em {matricula.CursoNome}.{detalhe}");
        }

        /// <summary>O próprio aluno reabre a matrícula que trancou e volta a acessar o curso.</summary>
        public static async Task ReabrirMatriculaAsync(Usuario aluno, Matricula matricula)
        {
            if (matricula.Status != StatusMatricula.Trancada)
                throw new InvalidOperationException("Só uma matrícula trancada pode ser reaberta.");

            matricula.Status = StatusMatricula.Ativa;
            matricula.TrancadaEm = null;
            await MatriculaRepositorio.AtualizarAsync(matricula);

            await NotificacaoService.ParaCoordenacaoAsync("Matrícula reaberta",
                $"{aluno.Nome} reabriu a matrícula em {matricula.CursoNome}.");
        }

        /// <summary>Dá ao aluno acesso ao curso (reativa a matrícula antiga, se houver) e libera a conta.</summary>
        public static async Task LiberarCursoAsync(Usuario aluno, string cursoId)
        {
            var existente = await MatriculaRepositorio.PorAlunoECursoAsync(aluno.Id, cursoId);
            if (existente == null)
            {
                await MatriculaRepositorio.InserirAsync(NovaMatricula(aluno.Id, cursoId));
            }
            else
            {
                existente.Status = StatusMatricula.Ativa;
                existente.LiberadaPorId = SessionService.UsuarioAtual?.Id;
                await MatriculaRepositorio.AtualizarAsync(existente);
            }

            await LiberarContaAsync(aluno);
            await NotificacaoService.ParaUsuarioAsync(aluno.Id, "Acesso liberado", "A coordenação liberou o seu acesso a um novo curso.");
        }

        /// <summary>Encerra ou reativa uma matrícula; reativar também libera a conta do aluno.</summary>
        public static async Task AlterarMatriculaAsync(Usuario aluno, Matricula matricula, StatusMatricula status)
        {
            matricula.Status = status;
            if (status == StatusMatricula.Ativa)
                matricula.TrancadaEm = null;
            await MatriculaRepositorio.AtualizarAsync(matricula);

            if (status == StatusMatricula.Ativa)
                await LiberarContaAsync(aluno);

            await NotificacaoService.ParaUsuarioAsync(aluno.Id,
                status == StatusMatricula.Ativa ? "Matrícula reativada" : "Matrícula encerrada",
                status == StatusMatricula.Ativa
                    ? $"A coordenação reativou a sua matrícula em {matricula.CursoNome}."
                    : $"A coordenação encerrou o seu acesso a {matricula.CursoNome}. O progresso fica guardado.");
        }

        /// <summary>Remove o acesso do aluno ao app, encerrando as matrículas ativas (o progresso fica guardado).</summary>
        public static async Task RemoverAcessoAlunoAsync(Usuario aluno, IEnumerable<Matricula> matriculas)
        {
            foreach (var matricula in matriculas.Where(m => m.Status == StatusMatricula.Ativa))
            {
                matricula.Status = StatusMatricula.Cancelada;
                await MatriculaRepositorio.AtualizarAsync(matricula);
            }

            aluno.StatusAcesso = StatusAcesso.Removido;
            await UsuarioRepositorio.AtualizarAsync(aluno);
        }

        /// <summary>Remove ou restaura o acesso de um professor (provas e aulas criadas por ele continuam no curso).</summary>
        public static Task DefinirAcessoProfessorAsync(Usuario professor, bool liberado)
        {
            professor.StatusAcesso = liberado ? StatusAcesso.Liberado : StatusAcesso.Removido;
            return UsuarioRepositorio.AtualizarAsync(professor);
        }

        /// <summary>Transforma uma conta de aluno sem matrícula em conta de professor, com acesso liberado.</summary>
        public static Task TornarProfessorAsync(Usuario usuario)
        {
            usuario.Roles = Roles.Professor;
            usuario.StatusAcesso = StatusAcesso.Liberado;
            return UsuarioRepositorio.AtualizarAsync(usuario);
        }

        /// <summary>A coordenação define uma senha provisória: o usuário cria a própria no próximo login.</summary>
        public static Task RedefinirSenhaAsync(Usuario usuario, string senha)
            => UsuarioRepositorio.AtualizarSenhaAsync(usuario.Id, SenhaService.GerarHash(senha), provisoria: true);

        /// <summary>Texto da situação do aluno ("Ativo", "Pendente", "Ex-aluno"...) usado nas listas da coordenação.</summary>
        public static string Situacao(Usuario aluno, IReadOnlyCollection<Matricula> matriculas) => aluno.StatusAcesso switch
        {
            StatusAcesso.Pendente => "Pendente",
            StatusAcesso.Removido => matriculas.Count > 0 ? "Ex-aluno" : "Sem acesso",
            _ when matriculas.Any(m => m.Status == StatusMatricula.Ativa) => "Ativo",
            _ => matriculas.Count > 0 ? "Ex-aluno" : "Sem curso"
        };

        private static async Task LiberarContaAsync(Usuario aluno)
        {
            if (aluno.StatusAcesso == StatusAcesso.Liberado)
                return;

            aluno.StatusAcesso = StatusAcesso.Liberado;
            await UsuarioRepositorio.AtualizarAsync(aluno);
        }

        private static Matricula NovaMatricula(string alunoId, string cursoId) => new()
        {
            AlunoId = alunoId,
            CursoId = cursoId,
            Status = StatusMatricula.Ativa,
            LiberadaPorId = SessionService.UsuarioAtual?.Id,
            DataMatricula = DateTime.Now
        };
    }
}
