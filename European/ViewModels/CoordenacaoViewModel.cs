using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CoordenacaoViewModel : BaseViewModel
    {
        private string _aba = "cursos";
        private string _filtroAlunos = "todos";
        private string _busca = string.Empty;
        private string _saudacao = string.Empty;
        private int _totalPendentes;
        private List<PessoaItem> _todosAlunos = new();

        public CoordenacaoViewModel()
        {
            SelecionarAbaCommand = new Command<string>(a => Aba = a);
            FiltrarAlunosCommand = new Command<string>(f => FiltroAlunos = f);
            AbrirPerfilCommand = new Command(async () => await Shell.Current.GoToAsync("perfil"));

            NovoCursoCommand = new Command(async () => await Ir("cursoedit", "Novo", true));
            AbrirCursoCommand = new Command<CursoResumo>(async c => await Ir("curso", "Curso", c?.Curso));
            ExcluirCursoCommand = new Command<CursoResumo>(async c => await ExcluirCursoAsync(c));

            LiberarPorEmailCommand = new Command(async () => await LiberarPorEmailAsync());
            CriarContaAlunoCommand = new Command(async () => await Ir("contanova", "Role", Roles.Aluno));
            AbrirAlunoCommand = new Command<PessoaItem>(async p => await Ir("alunoacesso", "Usuario", p?.Usuario));

            AdicionarProfessorCommand = new Command(async () => await AdicionarProfessorAsync());
            AbrirProfessorCommand = new Command<PessoaItem>(async p => await Ir("professoracesso", "Usuario", p?.Usuario));
        }

        public string Aba
        {
            get => _aba;
            set
            {
                if (SetProperty(ref _aba, value))
                {
                    OnPropertyChanged(nameof(MostrarCursos));
                    OnPropertyChanged(nameof(MostrarAlunos));
                    OnPropertyChanged(nameof(MostrarProfessores));
                }
            }
        }

        public bool MostrarCursos => Aba == "cursos";
        public bool MostrarAlunos => Aba == "alunos";
        public bool MostrarProfessores => Aba == "professores";

        public string FiltroAlunos
        {
            get => _filtroAlunos;
            set
            {
                if (SetProperty(ref _filtroAlunos, value))
                {
                    OnPropertyChanged(nameof(FiltroPendentes));
                    OnPropertyChanged(nameof(FiltroAtivos));
                    OnPropertyChanged(nameof(FiltroTodos));
                    AplicarFiltroAlunos();
                }
            }
        }

        public bool FiltroPendentes => FiltroAlunos == "pendentes";
        public bool FiltroAtivos => FiltroAlunos == "ativos";
        public bool FiltroTodos => FiltroAlunos == "todos";

        public string Busca
        {
            get => _busca;
            set
            {
                if (SetProperty(ref _busca, value))
                    AplicarFiltroAlunos();
            }
        }

        public string Saudacao { get => _saudacao; private set => SetProperty(ref _saudacao, value); }

        public int TotalPendentes
        {
            get => _totalPendentes;
            private set
            {
                if (SetProperty(ref _totalPendentes, value))
                {
                    OnPropertyChanged(nameof(PendentesTexto));
                    OnPropertyChanged(nameof(TemPendentes));
                }
            }
        }

        public string PendentesTexto => $"Pendentes ({TotalPendentes})";
        public bool TemPendentes => TotalPendentes > 0;

        public ObservableCollection<CursoResumo> Cursos { get; } = new();
        public ObservableCollection<PessoaItem> Alunos { get; } = new();
        public ObservableCollection<PessoaItem> Professores { get; } = new();

        public ICommand SelecionarAbaCommand { get; }
        public ICommand FiltrarAlunosCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand NovoCursoCommand { get; }
        public ICommand AbrirCursoCommand { get; }
        public ICommand ExcluirCursoCommand { get; }
        public ICommand LiberarPorEmailCommand { get; }
        public ICommand CriarContaAlunoCommand { get; }
        public ICommand AbrirAlunoCommand { get; }
        public ICommand AdicionarProfessorCommand { get; }
        public ICommand AbrirProfessorCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var usuario = SessionService.UsuarioAtual;
            Saudacao = $"Olá, {usuario?.Nome.Split(' ')[0]}";

            var cursos = await CursoRepositorio.ListarAsync();
            var ativosPorCurso = await MatriculaRepositorio.AtivasPorCursoAsync();
            Cursos.Clear();
            foreach (var curso in cursos)
            {
                var ativos = ativosPorCurso.GetValueOrDefault(curso.Id);
                Cursos.Add(new CursoResumo
                {
                    Curso = curso,
                    Detalhe = $"{ativos} aluno(s) ativos · {curso.ProfessorIds.Count} professor(es)" + (curso.IsPublicado ? string.Empty : " · inativo")
                });
            }

            var nomesCursos = cursos.ToDictionary(c => c.Id, c => c.Nome);

            var alunos = await UsuarioRepositorio.ListarAsync(Roles.Aluno);
            var matriculas = (await MatriculaRepositorio.DosAlunosAsync(alunos.Select(a => a.Id))).ToLookup(m => m.AlunoId);
            _todosAlunos = alunos.Select(a => ItemAluno(a, matriculas[a.Id].ToList(), nomesCursos)).ToList();
            _todosAlunos.ForEach(a => a.Abrir = AbrirAlunoCommand);
            TotalPendentes = alunos.Count(a => a.StatusAcesso == StatusAcesso.Pendente);

            if (TotalPendentes > 0 && FiltroAlunos == "todos" && Alunos.Count == 0)
                _filtroAlunos = "pendentes";
            OnPropertyChanged(nameof(FiltroPendentes));
            OnPropertyChanged(nameof(FiltroAtivos));
            OnPropertyChanged(nameof(FiltroTodos));
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

        public static PessoaItem ItemAluno(Usuario aluno, List<Matricula> matriculas, IReadOnlyDictionary<string, string> nomesCursos)
        {
            var ativas = matriculas.Where(m => m.Status == StatusMatricula.Ativa).ToList();
            var (status, alerta, destaque) = aluno.StatusAcesso switch
            {
                StatusAcesso.Pendente => ("Pendente", true, true),
                StatusAcesso.Removido => (matriculas.Count > 0 ? "Ex-aluno" : "Sem acesso", true, false),
                _ when ativas.Count > 0 => ("Ativo", false, false),
                _ => (matriculas.Count > 0 ? "Ex-aluno" : "Sem curso", true, false)
            };

            var cursos = (ativas.Count > 0 ? ativas : matriculas)
                .Select(m => nomesCursos.GetValueOrDefault(m.CursoId))
                .Where(n => n != null)
                .ToList();

            return new PessoaItem
            {
                Usuario = aluno,
                Status = status,
                Alerta = alerta,
                Destaque = destaque,
                Detalhe = cursos.Count > 0 ? string.Join(", ", cursos) : $"Cadastrado em {aluno.CreatedAt:dd/MM/yyyy}"
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

        private async Task ExcluirCursoAsync(CursoResumo? resumo)
        {
            if (resumo == null)
                return;

            if (!await ConfirmarAsync($"Excluir \"{resumo.Nome}\"?\n\nMatrículas, progresso, provas, notas e materiais do curso serão apagados. Certificados já emitidos são mantidos."))
                return;

            await ExecutarAsync(() => CursoRepositorio.ExcluirAsync(resumo.Curso.Id));
            await CarregarAsync();
        }

        private async Task LiberarPorEmailAsync()
        {
            var email = await Shell.Current.DisplayPromptAsync("Liberar acesso", "E-mail com que o aluno se cadastrou", "Buscar", "Cancelar",
                "aluno@email.com", keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email))
                return;

            Usuario? usuario = null;
            if (!await ExecutarAsync(async () => usuario = await UsuarioRepositorio.PorEmailAsync(email)))
                return;

            if (usuario == null)
            {
                if (await ConfirmarAsync("Nenhuma conta usa este e-mail. Deseja criar a conta do aluno agora?", "Criar conta"))
                    await Shell.Current.GoToAsync("contanova", new Dictionary<string, object> { ["Role"] = Roles.Aluno, ["Email"] = email.Trim() });
                return;
            }

            if (usuario.Roles != Roles.Aluno)
            {
                await AvisarAsync("Liberar acesso", "Este e-mail pertence a um professor ou à coordenação.");
                return;
            }

            await Ir("alunoacesso", "Usuario", usuario);
        }

        private async Task AdicionarProfessorAsync()
        {
            var email = await Shell.Current.DisplayPromptAsync("Adicionar professor", "E-mail do professor", "Buscar", "Cancelar",
                "professor@email.com", keyboard: Keyboard.Email);
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
                    await Shell.Current.GoToAsync("contanova", new Dictionary<string, object> { ["Role"] = Roles.Professor, ["Email"] = email.Trim() });
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

                if (!await ConfirmarAsync($"{usuario.Nome} se cadastrou pelo app. Liberar esta conta como professor?", "Liberar"))
                    return;

                usuario.Roles = Roles.Professor;
                usuario.StatusAcesso = StatusAcesso.Liberado;
                if (!await ExecutarAsync(() => UsuarioRepositorio.AtualizarAsync(usuario)))
                    return;
            }

            await Ir("professoracesso", "Usuario", usuario);
        }

        private static async Task Ir(string rota, string chave, object? valor)
        {
            if (valor != null)
                await Shell.Current.GoToAsync(rota, new Dictionary<string, object> { [chave] = valor });
        }
    }
}
