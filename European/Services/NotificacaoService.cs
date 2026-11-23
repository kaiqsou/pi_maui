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

        /// <summary>
        /// Avisos do calendário de provas do aluno: "prova liberada" quando uma prova abre para ele, e lembretes quando
        /// faltam 3 dias e no último dia do prazo (só das que ele ainda não fez). Sem servidor para agendar, roda quando
        /// a tela inicial do aluno aparece; a Chave de cada aviso impede que ele se repita.
        /// </summary>
        public static async Task AvisarCalendarioDeProvasAsync(Usuario aluno)
        {
            try
            {
                var agora = DateTime.Now;
                var matriculas = await MatriculaRepositorio.AtivasDoAlunoAsync(aluno.Id);
                if (matriculas.Count == 0)
                    return;

                var feitas = (await ResultadoRepositorio.DoAlunoAsync(aluno.Id)).Select(r => r.ProvaId).ToHashSet();
                var abertas = (await ProvaService.AgendaAsync(matriculas))
                    .Where(p => p.Aberta && !feitas.Contains(p.Prova.Id))
                    .ToList();

                var avisos = new List<Notificacao>();
                foreach (var p in abertas)
                {
                    avisos.Add(new Notificacao
                    {
                        UsuarioId = aluno.Id,
                        Chave = $"prova-liberada:{p.Prova.Id}",
                        Titulo = "Nova prova liberada",
                        Mensagem = $"A prova \"{p.Prova.Titulo}\" já está disponível na aba Provas. Você tem até {p.PrazoTexto} para fazer."
                    });

                    if (p.PrazoAte > agora.AddDays(3))
                        continue;

                    var ultimoDia = p.PrazoAte <= agora.AddDays(1);
                    avisos.Add(new Notificacao
                    {
                        UsuarioId = aluno.Id,
                        Chave = $"prazo-{(ultimoDia ? "1d" : "3d")}:{p.Prova.Id}",
                        Titulo = ultimoDia ? "Prova encerra em breve" : "Prazo de prova se aproximando",
                        Mensagem = $"A prova \"{p.Prova.Titulo}\" encerra em {p.PrazoTexto}" +
                                   (ultimoDia ? ". Faça antes que o prazo termine; depois disso ela conta como nota 0." : ". Não deixe para a última hora.")
                    });
                }

                if (avisos.Count == 0)
                    return;

                var recebidas = await NotificacaoRepositorio.ChavesRecebidasAsync(aluno.Id, avisos.Select(n => n.Chave!));
                await NotificacaoRepositorio.InserirAsync(avisos.Where(n => !recebidas.Contains(n.Chave!)));
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(NotificacaoService), ex);
            }
        }

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
