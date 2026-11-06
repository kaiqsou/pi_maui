using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Sala (turma) do professor. Ele só enxerga e adiciona alunos matriculados nos cursos que leciona;
    /// com um curso escolhido na sala, só os alunos desse curso.
    /// </summary>
    public partial class SalaEditViewModel : BaseViewModel
    {
        private const int MaxSugestoes = 8;

        /// <summary>Alunos com matrícula ativa nos cursos do professor, com os cursos de cada um.</summary>
        private List<(Usuario Aluno, HashSet<string> CursoIds)> _alunosDosCursos = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEdicao))]
        private partial Sala? SalaAtual { get; set; }

        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descricao { get; set; } = string.Empty;

        [ObservableProperty]
        public partial List<CursoOpcao> Cursos { get; private set; } = new();

        [ObservableProperty]
        public partial CursoOpcao? CursoSelecionado { get; set; }

        /// <summary>Nome ou e-mail digitado; as sugestões aparecem enquanto o professor digita.</summary>
        [ObservableProperty]
        public partial string Busca { get; set; } = string.Empty;

        public ObservableCollection<PessoaItem> Alunos { get; } = new();
        public ObservableCollection<PessoaItem> Sugestoes { get; } = new();
        public bool TemSugestoes => Sugestoes.Count > 0;
        public bool SemResultado => Busca.Trim().Length >= 2 && Sugestoes.Count == 0;
        public string ResumoAlunos => $"{Alunos.Count} aluno(s) na sala";
        public bool IsEdicao => SalaAtual != null;

        partial void OnCursoSelecionadoChanged(CursoOpcao? value) => AtualizarSugestoes();
        partial void OnBuscaChanged(string value) => AtualizarSugestoes();

        protected override async Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            var sala = parametros.Obter<Sala>("Sala");
            SalaAtual = sala;
            Title = sala == null ? "Nova sala" : "Editar sala";
            Nome = sala?.Nome ?? string.Empty;
            Descricao = sala?.Descricao ?? string.Empty;

            await ExecutarAsync(async () =>
            {
                var cursosDoProfessor = await CursoRepositorio.DoProfessorAsync(professor.Id);
                var opcoes = new List<CursoOpcao> { new() { Texto = "Todos os meus cursos" } };
                opcoes.AddRange(cursosDoProfessor.Select(c => new CursoOpcao { Valor = c, Texto = c.Nome }));
                Cursos = opcoes;

                // alunos com matrícula ativa em algum curso que o professor leciona
                var matriculas = new List<Matricula>();
                foreach (var curso in cursosDoProfessor)
                    matriculas.AddRange((await MatriculaRepositorio.DoCursoAsync(curso.Id)).Where(m => m.Status == StatusMatricula.Ativa));
                var cursosPorAluno = matriculas.GroupBy(m => m.AlunoId).ToDictionary(g => g.Key, g => g.Select(m => m.CursoId).ToHashSet());
                _alunosDosCursos = (await UsuarioRepositorio.PorIdsAsync(cursosPorAluno.Keys))
                    .Where(a => a.Roles == Roles.Aluno)
                    .OrderBy(a => a.Nome)
                    .Select(a => (a, cursosPorAluno[a.Id]))
                    .ToList();

                Alunos.Clear();
                if (sala != null)
                {
                    foreach (var aluno in (await UsuarioRepositorio.PorIdsAsync(sala.AlunoIds)).OrderBy(a => a.Nome))
                        Alunos.Add(Item(aluno));
                }

                CursoSelecionado = opcoes.FirstOrDefault(o => o.Valor?.Id == sala?.CursoId) ?? opcoes[0];
                Busca = string.Empty;
                OnPropertyChanged(nameof(ResumoAlunos));
                AtualizarSugestoes();
            });
        }

        private static PessoaItem Item(Usuario aluno) => new()
        {
            Usuario = aluno,
            Status = aluno.AcessoLiberado ? "Ativo" : aluno.StatusAcesso == StatusAcesso.Pendente ? "Pendente" : "Sem acesso",
            Alerta = !aluno.AcessoLiberado
        };

        /// <summary>Alunos que o professor pode pôr nesta sala: dos cursos dele (ou do curso da sala) e que ainda não estão nela.</summary>
        private IEnumerable<Usuario> Disponiveis()
        {
            var cursoId = CursoSelecionado?.Valor?.Id;
            var naSala = Alunos.Select(a => a.Usuario.Id).ToHashSet();
            return _alunosDosCursos
                .Where(a => cursoId == null || a.CursoIds.Contains(cursoId))
                .Where(a => !naSala.Contains(a.Aluno.Id))
                .Select(a => a.Aluno);
        }

        private void AtualizarSugestoes()
        {
            var termo = Busca.Trim();
            Sugestoes.Clear();
            if (termo.Length >= 2)
            {
                var achados = Disponiveis()
                    .Where(a => a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                                || a.Email.Contains(termo, StringComparison.OrdinalIgnoreCase))
                    .Take(MaxSugestoes);
                foreach (var aluno in achados)
                    Sugestoes.Add(Item(aluno));
            }

            OnPropertyChanged(nameof(TemSugestoes));
            OnPropertyChanged(nameof(SemResultado));
        }

        [RelayCommand]
        private void AdicionarSugestao(PessoaItem? item)
        {
            if (item == null || Alunos.Any(a => a.Usuario.Id == item.Usuario.Id))
                return;

            Alunos.Add(item);
            Busca = string.Empty;
            OnPropertyChanged(nameof(ResumoAlunos));
        }

        [RelayCommand]
        private void RemoverAluno(PessoaItem? item)
        {
            if (item == null)
                return;

            Alunos.Remove(item);
            OnPropertyChanged(nameof(ResumoAlunos));
            AtualizarSugestoes();
        }

        /// <summary>Enter no campo: adiciona pelo e-mail exato, ou a única sugestão encontrada.</summary>
        [RelayCommand]
        private async Task AdicionarAlunoAsync()
        {
            var termo = Busca.Trim();
            if (termo.Length == 0)
                return;

            var email = UsuarioRepositorio.NormalizarEmail(termo);
            if (Alunos.Any(a => a.Email == email))
            {
                await AvisarAsync("Sala", "Este aluno já está na sala.");
                return;
            }

            var aluno = Disponiveis().FirstOrDefault(a => a.Email == email);
            if (aluno != null)
            {
                AdicionarSugestao(Item(aluno));
                return;
            }

            if (Sugestoes.Count == 1)
            {
                AdicionarSugestao(Sugestoes[0]);
                return;
            }

            await AvisarAsync("Sala", Sugestoes.Count > 1
                ? "Mais de um aluno encontrado. Toque no aluno desejado na lista."
                : "Nenhum aluno dos seus cursos com este nome ou e-mail.");
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            if (string.IsNullOrWhiteSpace(Nome))
            {
                await AvisarAsync("Sala", "Informe o nome da sala.");
                return;
            }

            var sala = SalaAtual ?? new Sala { ProfessorId = professor.Id };
            sala.Nome = Nome.Trim();
            sala.Descricao = Descricao.Trim();
            sala.CursoId = CursoSelecionado?.Valor?.Id;
            sala.AlunoIds = Alunos.Select(a => a.Usuario.Id).ToList();

            await SalvarEVoltarAsync(() => SalaRepositorio.SalvarAsync(sala));
        }

        [RelayCommand]
        private Task Excluir()
            => SalaAtual == null
                ? Task.CompletedTask
                : ExcluirEVoltarAsync($"Excluir a sala \"{SalaAtual.Nome}\"? Provas direcionadas a ela passarão a valer para todo o curso.",
                    () => SalaRepositorio.ExcluirAsync(SalaAtual.Id));
    }
}
