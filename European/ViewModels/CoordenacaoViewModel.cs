using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Painel da coordenação, com três abas: cursos, alunos (e solicitações de matrícula) e professores.</summary>
    public partial class CoordenacaoViewModel : BaseViewModel
    {
        private List<PessoaItem> _todosAlunos = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarCursos), nameof(MostrarAlunos), nameof(MostrarProfessores))]
        public partial string Aba { get; set; } = "cursos";

        public bool MostrarCursos => Aba == "cursos";
        public bool MostrarAlunos => Aba == "alunos";
        public bool MostrarProfessores => Aba == "professores";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FiltroPendentes), nameof(FiltroAtivos), nameof(FiltroTodos))]
        public partial string FiltroAlunos { get; set; } = "todos";

        public bool FiltroPendentes => FiltroAlunos == "pendentes";
        public bool FiltroAtivos => FiltroAlunos == "ativos";
        public bool FiltroTodos => FiltroAlunos == "todos";

        [ObservableProperty]
        public partial string Busca { get; set; } = string.Empty;

        /// <summary>Notificações não lidas (sino no cabeçalho).</summary>
        public Sino Sino { get; } = new();

        [ObservableProperty]
        public partial string Saudacao { get; private set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PendentesTexto))]
        public partial int TotalPendentes { get; private set; }

        public string PendentesTexto => $"Pendentes ({TotalPendentes})";

        public ObservableCollection<CursoResumo> Cursos { get; } = new();
        public ObservableCollection<PessoaItem> Alunos { get; } = new();
        public ObservableCollection<PessoaItem> Professores { get; } = new();

        partial void OnFiltroAlunosChanged(string value) => AplicarFiltroAlunos();
        partial void OnBuscaChanged(string value) => AplicarFiltroAlunos();

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            Saudacao = Formatos.Saudacao(SessionService.UsuarioAtual?.Nome);
            await Sino.AtualizarAsync();

            var cursos = await CursoRepositorio.ListarAsync();
            var ativosPorCurso = await MatriculaRepositorio.AtivasPorCursoAsync();
            Cursos.Clear();
            foreach (var curso in cursos)
            {
                Cursos.Add(new CursoResumo
                {
                    Curso = curso,
                    Detalhe = $"{ativosPorCurso.GetValueOrDefault(curso.Id)} aluno(s) ativos · {curso.ProfessorIds.Count} professor(es)"
                              + (curso.IsPublicado ? string.Empty : " · inativo")
                });
            }

            var nomesCursos = cursos.ToDictionary(c => c.Id, c => c.Nome);

            // solicitações de matrícula (candidatos que ainda não são usuários) aparecem primeiro
            var solicitacoes = await SolicitacaoRepositorio.ListarAsync();
            var alunos = await UsuarioRepositorio.ListarAsync(Roles.Aluno);
            var matriculas = (await MatriculaRepositorio.DosAlunosAsync(alunos.Select(a => a.Id))).ToLookup(m => m.AlunoId);
            _todosAlunos = solicitacoes.Select(s => ItemSolicitacao(s, nomesCursos))
                .Concat(alunos.Select(a => ItemAluno(a, matriculas[a.Id].ToList(), nomesCursos)))
                .ToList();

            TotalPendentes = alunos.Count(a => a.StatusAcesso == StatusAcesso.Pendente) + solicitacoes.Count;

            // na primeira carga, com pendências, a lista já abre filtrada nelas
            if (TotalPendentes > 0 && FiltroAlunos == "todos" && Alunos.Count == 0)
                FiltroAlunos = "pendentes";
            else
                AplicarFiltroAlunos();

            var professores = await UsuarioRepositorio.ListarAsync(Roles.Professor);
            Professores.Clear();
            foreach (var professor in professores)
            {
                var doProfessor = cursos.Where(c => c.ProfessorIds.Contains(professor.Id)).Select(c => c.Nome).ToList();
                Professores.Add(new PessoaItem
                {
                    Usuario = professor,
                    Status = professor.AcessoLiberado ? "Ativo" : "Sem acesso",
                    Alerta = !professor.AcessoLiberado,
                    Detalhe = doProfessor.Count == 0 ? "Nenhum curso atribuído" : string.Join(", ", doProfessor),
                    Abrir = AbrirProfessorCommand
                });
            }
        });

        private PessoaItem ItemAluno(Usuario aluno, List<Matricula> matriculas, IReadOnlyDictionary<string, string> nomesCursos)
        {
            var status = ContaService.Situacao(aluno, matriculas);
            var ativas = matriculas.Where(m => m.Status == StatusMatricula.Ativa).ToList();
            var cursos = (ativas.Count > 0 ? ativas : matriculas)
                .Select(m => nomesCursos.GetValueOrDefault(m.CursoId))
                .Where(n => n != null)
                .ToList();

            return new PessoaItem
            {
                Usuario = aluno,
                Status = status,
                Alerta = status != "Ativo",
                Destaque = aluno.StatusAcesso == StatusAcesso.Pendente,
                Detalhe = cursos.Count > 0 ? string.Join(", ", cursos) : $"Cadastrado em {aluno.CreatedAt:dd/MM/yyyy}",
                Abrir = AbrirAlunoCommand
            };
        }

        private PessoaItem ItemSolicitacao(SolicitacaoMatricula solicitacao, IReadOnlyDictionary<string, string> nomesCursos)
        {
            var (status, destaque) = solicitacao switch
            {
                { TemPendencia: true } => ("Com pendência", false),
                { Pago: false } => ("Aguardando pagamento", false),
                _ => ("Solicitação", true)
            };

            return new PessoaItem
            {
                // não é usuário: o Usuario aqui só alimenta nome, e-mail e o filtro de pendentes
                Usuario = new Usuario { Nome = solicitacao.Nome, Email = solicitacao.Email, StatusAcesso = StatusAcesso.Pendente },
                Solicitacao = solicitacao,
                Status = status,
                Alerta = true,
                Destaque = destaque,
                Detalhe = $"{nomesCursos.GetValueOrDefault(solicitacao.CursoId) ?? "Curso removido"} · {solicitacao.CriadaEm:dd/MM/yyyy}",
                Abrir = AbrirSolicitacaoCommand
            };
        }

        private void AplicarFiltroAlunos()
        {
            var termo = Busca.Trim();
            var filtrados = _todosAlunos
                .Where(a => FiltroAlunos switch
                {
                    "pendentes" => a.Usuario.StatusAcesso == StatusAcesso.Pendente,
                    "ativos" => a.Status == "Ativo",
                    _ => true
                })
                .Where(a => termo.Length == 0
                            || a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                            || a.Email.Contains(termo, StringComparison.OrdinalIgnoreCase));

            Alunos.Clear();
            foreach (var aluno in filtrados)
                Alunos.Add(aluno);
        }

        // ---- Navegação ----

        [RelayCommand]
        private void SelecionarAba(string aba) => Aba = aba;

        [RelayCommand]
        private void FiltrarAlunos(string filtro) => FiltroAlunos = filtro;

        [RelayCommand]
        private Task AbrirPerfil() => Navegacao.IrAsync(Navegacao.Perfil);

        [RelayCommand]
        private Task NovoCurso() => Navegacao.IrAsync(Navegacao.CursoEdit);

        [RelayCommand]
        private Task AbrirCurso(CursoResumo? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.Curso, ("Curso", item.Curso));

        [RelayCommand]
        private Task CriarContaAluno() => Navegacao.IrAsync(Navegacao.ContaNova, ("Role", Roles.Aluno));

        [RelayCommand]
        private Task AbrirAluno(PessoaItem? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.AlunoAcesso, ("Usuario", item.Usuario));

        [RelayCommand]
        private Task AbrirSolicitacao(PessoaItem? item)
            => item?.Solicitacao == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.Solicitacao, ("Solicitacao", item.Solicitacao));

        [RelayCommand]
        private Task AbrirProfessor(PessoaItem? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ProfessorAcesso, ("Usuario", item.Usuario));

        // ---- Buscar por e-mail ----

        /// <summary>Busca o aluno (ou o candidato) pelo e-mail e abre a tela certa; oferece criar a conta se não existir.</summary>
        [RelayCommand]
        private async Task LiberarPorEmailAsync()
        {
            var email = await PerguntarAsync("Liberar acesso", "E-mail com que o aluno se cadastrou", "Buscar", "aluno@email.com", teclado: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email))
                return;

            Usuario? usuario = null;
            SolicitacaoMatricula? solicitacao = null;
            if (!await ExecutarAsync(async () =>
                {
                    usuario = await UsuarioRepositorio.PorEmailAsync(email);
                    if (usuario == null)
                        solicitacao = await SolicitacaoRepositorio.PorEmailAsync(email);
                }))
                return;

            // candidato que se cadastrou pelo app: abre a solicitação de matrícula
            if (solicitacao != null)
            {
                await Navegacao.IrAsync(Navegacao.Solicitacao, ("Solicitacao", solicitacao));
                return;
            }

            if (usuario == null)
            {
                if (await ConfirmarAsync("Nenhuma conta usa este e-mail. Deseja criar a conta do aluno agora?", "Criar conta"))
                    await Navegacao.IrAsync(Navegacao.ContaNova, ("Role", Roles.Aluno), ("Email", email.Trim()));
                return;
            }

            if (usuario.Roles != Roles.Aluno)
            {
                await AvisarAsync("Liberar acesso", "Este e-mail pertence a um professor ou à coordenação.");
                return;
            }

            await Navegacao.IrAsync(Navegacao.AlunoAcesso, ("Usuario", usuario));
        }

        /// <summary>Busca a conta pelo e-mail: abre o professor, transforma um aluno sem matrícula em professor ou oferece criar a conta.</summary>
        [RelayCommand]
        private async Task AdicionarProfessorAsync()
        {
            var email = await PerguntarAsync("Adicionar professor", "E-mail do professor", "Buscar", "professor@email.com", teclado: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email))
                return;

            Usuario? usuario = null;
            var matriculas = 0;
            if (!await ExecutarAsync(async () =>
                {
                    usuario = await UsuarioRepositorio.PorEmailAsync(email);
                    if (usuario != null)
                        matriculas = (await MatriculaRepositorio.DoAlunoAsync(usuario.Id)).Count;
                }))
                return;

            if (usuario == null)
            {
                if (await ConfirmarAsync("Nenhuma conta usa este e-mail. Deseja criar a conta do professor agora?", "Criar conta"))
                    await Navegacao.IrAsync(Navegacao.ContaNova, ("Role", Roles.Professor), ("Email", email.Trim()));
                return;
            }

            if (usuario.IsCoordenacao)
            {
                await AvisarAsync("Adicionar professor", "Este e-mail pertence à coordenação.");
                return;
            }

            if (usuario.Roles == Roles.Aluno)
            {
                if (matriculas > 0)
                {
                    await AvisarAsync("Adicionar professor", "Esta conta pertence a um aluno com matrícula e não pode virar conta de professor.");
                    return;
                }

                if (!await ConfirmarAsync($"{usuario.Nome} se cadastrou pelo app. Liberar esta conta como professor?", "Liberar")
                    || !await ExecutarAsync(() => ContaService.TornarProfessorAsync(usuario)))
                    return;
            }

            await Navegacao.IrAsync(Navegacao.ProfessorAcesso, ("Usuario", usuario));
        }
    }
}
