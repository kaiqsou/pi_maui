using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Coordenação cria a conta de um aluno (já com acesso ao curso) ou de um professor.</summary>
    public partial class ContaNovaViewModel : BaseViewModel
    {
        private const string IconeCopiar = "";  // content_copy
        private const string IconeCopiado = ""; // check

        private Roles _role = Roles.Aluno;

        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Senha { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CopiarIcone { get; private set; } = IconeCopiar;

        [ObservableProperty]
        public partial List<CursoOpcao> Cursos { get; private set; } = new();

        [ObservableProperty]
        public partial CursoOpcao? CursoSelecionado { get; set; }

        public bool IsAluno => _role == Roles.Aluno;
        public string Descricao => IsAluno
            ? "Para alunos que ainda não têm conta. A conta já nasce com acesso ao curso escolhido."
            : "A conta do professor é criada com acesso liberado. Depois, escolha os cursos que ele leciona.";

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _role = parametros.Obter<Roles?>("Role") ?? Roles.Aluno;
            Title = IsAluno ? "Criar conta de aluno" : "Criar conta de professor";
            Email = parametros.Obter<string>("Email") ?? string.Empty;
            Senha = ContaService.GerarSenhaProvisoria();
            OnPropertyChanged(nameof(IsAluno));
            OnPropertyChanged(nameof(Descricao));

            return ExecutarAsync(async () =>
            {
                Cursos = (await CursoRepositorio.ListarAsync()).Where(c => c.IsPublicado)
                    .Select(c => new CursoOpcao { Valor = c, Texto = c.Nome }).ToList();
                CursoSelecionado = Cursos.FirstOrDefault();
            });
        }

        [RelayCommand]
        private void GerarSenha() => Senha = ContaService.GerarSenhaProvisoria();

        [RelayCommand]
        private async Task CopiarSenhaAsync()
        {
            if (string.IsNullOrEmpty(Senha))
                return;

            await Clipboard.Default.SetTextAsync(Senha);
            CopiarIcone = IconeCopiado;
            await Task.Delay(1500);
            CopiarIcone = IconeCopiar;
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || !ContaService.EmailValido(Email))
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
            if (!await ExecutarAsync(async () => criado = await ContaService.CriarContaAsync(usuario, IsAluno ? CursoSelecionado!.Valor!.Id : null)))
                return;

            if (!criado)
            {
                await AvisarAsync("Conta", "Já existe uma conta com este e-mail. Use \"Liberar por e-mail\".");
                return;
            }

            await AvisarAsync("Conta criada",
                $"Envie os dados de acesso para {usuario.Nome}:\n\nE-mail: {usuario.Email}\nSenha provisória: {Senha}");

            // professor: segue direto para a tela dele, para escolher os cursos que leciona
            if (IsAluno)
                await VoltarAsync();
            else
                await Navegacao.IrAsync($"../{Navegacao.ProfessorAcesso}", ("Usuario", usuario));
        }
    }
}
