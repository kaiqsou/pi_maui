using System.Security.Cryptography;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Coordenação cria a conta de um aluno (já com acesso ao curso) ou de um professor.</summary>
    public class ContaNovaViewModel : BaseViewModel
    {
        private Roles _role = Roles.Aluno;
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private List<CursoOpcao> _cursos = new();
        private CursoOpcao? _cursoSelecionado;

        public ContaNovaViewModel()
        {
            GerarSenhaCommand = new Command(() => Senha = GerarSenha());
            CopiarSenhaCommand = new Command(async () => await CopiarSenhaAsync());
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        private const string IconeCopiar = "";   // content_copy
        private const string IconeCopiado = "";  // check
        private string _copiarIcone = IconeCopiar;

        public string CopiarIcone { get => _copiarIcone; private set => SetProperty(ref _copiarIcone, value); }
        public ICommand CopiarSenhaCommand { get; }

        private async Task CopiarSenhaAsync()
        {
            if (string.IsNullOrEmpty(Senha))
                return;

            await Clipboard.Default.SetTextAsync(Senha);
            CopiarIcone = IconeCopiado;
            await Task.Delay(1500);
            CopiarIcone = IconeCopiar;
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
        public List<CursoOpcao> Cursos { get => _cursos; private set => SetProperty(ref _cursos, value); }
        public CursoOpcao? CursoSelecionado { get => _cursoSelecionado; set => SetProperty(ref _cursoSelecionado, value); }

        public bool IsAluno => _role == Roles.Aluno;
        public string Descricao => IsAluno
            ? "Para alunos que ainda não têm conta. A conta já nasce com acesso ao curso escolhido."
            : "A conta do professor é criada com acesso liberado. Depois, escolha os cursos que ele leciona.";

        public ICommand GerarSenhaCommand { get; }
        public ICommand SalvarCommand { get; }

        public Task CarregarAsync(Roles role, string? email)
        {
            _role = role;
            Title = role == Roles.Aluno ? "Criar conta de aluno" : "Criar conta de professor";
            Email = email ?? string.Empty;
            Senha = GerarSenha();
            OnPropertyChanged(nameof(IsAluno));
            OnPropertyChanged(nameof(Descricao));

            return ExecutarAsync(async () =>
            {
                Cursos = (await CursoRepositorio.ListarAsync()).Where(c => c.IsPublicado)
                    .Select(c => new CursoOpcao { Valor = c, Texto = c.Nome }).ToList();
                CursoSelecionado = Cursos.FirstOrDefault();
            });
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || !UserViewModel.EmailValido(Email))
            {
                await AvisarAsync("Conta", "Informe o nome e um e-mail válido.");
                return;
            }

            if (!SenhaService.EhValida(Senha, out var erroSenha))
            {
                await AvisarAsync("Conta", erroSenha);
                return;
            }

            if (IsAluno && CursoSelecionado?.Valor == null)
            {
                await AvisarAsync("Conta", "Selecione o curso do aluno.");
                return;
            }

            var usuario = new Usuario
            {
                Nome = Nome,
                Email = Email,
                SenhaHash = SenhaService.GerarHash(Senha),
                SenhaProvisoria = true,
                Roles = _role,
                StatusAcesso = StatusAcesso.Liberado
            };

            var criado = false;
            var ok = await ExecutarAsync(async () =>
            {
                criado = await UsuarioRepositorio.InserirAsync(usuario);
                if (!criado)
                    return;

                if (IsAluno)
                {
                    await MatriculaRepositorio.InserirAsync(new Matricula
                    {
                        AlunoId = usuario.Id,
                        CursoId = CursoSelecionado!.Valor!.Id,
                        Status = StatusMatricula.Ativa,
                        LiberadaPorId = SessionService.UsuarioAtual?.Id
                    });
                }
            });

            if (!ok)
                return;

            if (!criado)
            {
                await AvisarAsync("Conta", "Já existe uma conta com este e-mail. Use \"Liberar por e-mail\".");
                return;
            }

            await AvisarAsync("Conta criada",
                $"Envie os dados de acesso para {usuario.Nome}:\n\nE-mail: {usuario.Email}\nSenha provisória: {Senha}");

            if (IsAluno)
                await VoltarAsync();
            else
                await Shell.Current.GoToAsync("../professoracesso", new Dictionary<string, object> { ["Usuario"] = usuario });
        }

        private static string GerarSenha()
        {
            const string letras = "abcdefghjkmnpqrstuvwxyz";
            const string numeros = "23456789";
            var chars = Enumerable.Range(0, 5).Select(_ => letras[RandomNumberGenerator.GetInt32(letras.Length)])
                .Concat(Enumerable.Range(0, 3).Select(_ => numeros[RandomNumberGenerator.GetInt32(numeros.Length)]));
            return new string(chars.ToArray());
        }
    }
}
