using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>
    /// Avisos entre aluno, professor e coordenação. Uma falha ao avisar nunca desfaz a operação principal
    /// (a matrícula, a prova, a correção...): o erro só vai para o log.
    /// </summary>
    public static class NotificacaoService
    {
        public static Task ParaUsuarioAsync(string usuarioId, string titulo, string mensagem)
            => EnviarAsync(() => Task.FromResult<IEnumerable<string>>(new[] { usuarioId }), titulo, mensagem);

        public static Task ParaUsuariosAsync(IEnumerable<string> usuarioIds, string titulo, string mensagem)
            => EnviarAsync(() => Task.FromResult(usuarioIds), titulo, mensagem);

        /// <summary>Para todas as contas da coordenação.</summary>
        public static Task ParaCoordenacaoAsync(string titulo, string mensagem)
            => EnviarAsync(async () => (await UsuarioRepositorio.CoordenacaoAsync()).Select(u => u.Id), titulo, mensagem);

        /// <summary>Para os professores atribuídos ao curso.</summary>
        public static Task ParaProfessoresDoCursoAsync(string cursoId, string titulo, string mensagem)
            => EnviarAsync(async () => (await CursoRepositorio.PorIdAsync(cursoId))?.ProfessorIds ?? new List<string>(), titulo, mensagem);

        /// <summary>Para os alunos com matrícula ativa no curso (ou só os da sala, se informada).</summary>
        public static Task ParaAlunosAsync(string cursoId, string? salaId, string titulo, string mensagem)
            => EnviarAsync(async () =>
            {
                var ativos = (await MatriculaRepositorio.DoCursoAsync(cursoId))
                    .Where(m => m.Status == StatusMatricula.Ativa)
                    .Select(m => m.AlunoId);
                if (salaId == null)
                    return ativos;

                var sala = await SalaRepositorio.PorIdAsync(salaId);
                return ativos.Intersect(sala?.AlunoIds ?? new List<string>());
            }, titulo, mensagem);

        private static async Task EnviarAsync(Func<Task<IEnumerable<string>>> destinatarios, string titulo, string mensagem)
        {
            try
            {
                var autor = SessionService.UsuarioAtual?.Id;
                var notificacoes = (await destinatarios())
                    .Where(id => !string.IsNullOrEmpty(id) && id != autor) // ninguém é avisado da própria ação
                    .Distinct()
                    .Select(id => new Notificacao { UsuarioId = id, Titulo = titulo, Mensagem = mensagem });
                await NotificacaoRepositorio.InserirAsync(notificacoes);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(NotificacaoService), ex);
            }
        }
    }
}
