using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CursoOpcao : Opcao<Curso> { }

    public class PessoaItem
    {
        public Usuario Usuario { get; init; } = new();
        public string Nome => Usuario.Nome;
        public string Email => Usuario.Email;
        public string Iniciais => Usuario.Iniciais;
        public string Status { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
        public bool Destaque { get; init; }
        public bool Alerta { get; init; }
        public ICommand? Abrir { get; set; }
    }

    public class SalaEditViewModel : BaseViewModel
    {
        private Sala? _sala;
        private string _nome = string.Empty;
        private string _descricao = string.Empty;
        private string _emailAluno = string.Empty;
        private List<CursoOpcao> _cursos = new();
        private CursoOpcao? _cursoSelecionado;

        public SalaEditViewModel()
        {
            AdicionarAlunoCommand = new Command(async () => await AdicionarAlunoAsync());
            RemoverAlunoCommand = new Command<PessoaItem>(p => { if (p != null) Alunos.Remove(p); OnPropertyChanged(nameof(ResumoAlunos)); });
            SalvarCommand = new Command(async () => await SalvarAsync());
            ExcluirCommand = new Command(async () => await ExcluirAsync());
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public string EmailAluno { get => _emailAluno; set => SetProperty(ref _emailAluno, value); }
        public List<CursoOpcao> Cursos { get => _cursos; private set => SetProperty(ref _cursos, value); }
        public CursoOpcao? CursoSelecionado { get => _cursoSelecionado; set => SetProperty(ref _cursoSelecionado, value); }

        public ObservableCollection<PessoaItem> Alunos { get; } = new();
        public string ResumoAlunos => $"{Alunos.Count} aluno(s) na sala";
        public bool IsEdicao => _sala != null;

        public ICommand AdicionarAlunoCommand { get; }
        public ICommand RemoverAlunoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }

        public async Task CarregarAsync(Sala? sala)
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            _sala = sala;
            Title = sala == null ? "Nova sala" : "Editar sala";
            Nome = sala?.Nome ?? string.Empty;
            Descricao = sala?.Descricao ?? string.Empty;
            OnPropertyChanged(nameof(IsEdicao));

            await ExecutarAsync(async () =>
            {
                var opcoes = new List<CursoOpcao> { new() { Texto = "Sem curso específico" } };
                opcoes.AddRange((await CursoRepositorio.DoProfessorAsync(professor.Id)).Select(c => new CursoOpcao { Valor = c, Texto = c.Nome }));
                Cursos = opcoes;
                CursoSelecionado = opcoes.FirstOrDefault(o => o.Valor?.Id == sala?.CursoId) ?? opcoes[0];

                Alunos.Clear();
                if (sala != null)
                {
                    foreach (var aluno in (await UsuarioRepositorio.PorIdsAsync(sala.AlunoIds)).OrderBy(a => a.Nome))
                        Alunos.Add(Item(aluno));
                }

                OnPropertyChanged(nameof(ResumoAlunos));
            });
        }

        private static PessoaItem Item(Usuario aluno) => new()
        {
            Usuario = aluno,
            Status = aluno.AcessoLiberado ? "Ativo" : aluno.StatusAcesso == StatusAcesso.Pendente ? "Pendente" : "Sem acesso",
            Alerta = !aluno.AcessoLiberado
        };

        private async Task AdicionarAlunoAsync()
        {
            var email = EmailAluno.Trim();
            if (string.IsNullOrEmpty(email))
                return;

            if (Alunos.Any(a => a.Email == UsuarioRepositorio.NormalizarEmail(email)))
            {
                await AvisarAsync("Sala", "Este aluno já está na sala.");
                return;
            }

            await ExecutarAsync(async () =>
            {
                var usuario = await UsuarioRepositorio.PorEmailAsync(email);
                if (usuario == null || usuario.Roles != Roles.Aluno)
                {
                    await AvisarAsync("Sala", "Nenhum aluno cadastrado na plataforma com este e-mail.");
                    return;
                }

                Alunos.Add(Item(usuario));
                EmailAluno = string.Empty;
                OnPropertyChanged(nameof(ResumoAlunos));

                if (!usuario.AcessoLiberado)
                    await AvisarAsync("Sala", $"{usuario.Nome} foi adicionado, mas ainda não tem acesso liberado pela coordenação.");
            });
        }

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

            var sala = _sala ?? new Sala { ProfessorId = professor.Id };
            sala.Nome = Nome.Trim();
            sala.Descricao = Descricao.Trim();
            sala.CursoId = CursoSelecionado?.Valor?.Id;
            sala.AlunoIds = Alunos.Select(a => a.Usuario.Id).ToList();

            if (await ExecutarAsync(() => SalaRepositorio.SalvarAsync(sala)))
                await VoltarAsync();
        }

        private async Task ExcluirAsync()
        {
            if (_sala == null || !await ConfirmarAsync($"Excluir a sala \"{_sala.Nome}\"? Provas direcionadas a ela passarão a valer para todo o curso."))
                return;

            if (await ExecutarAsync(() => SalaRepositorio.ExcluirAsync(_sala.Id)))
                await VoltarAsync();
        }
    }
}
