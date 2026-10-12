using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class PagamentoViewModel : BaseViewModel
    {
        private Curso? _curso;
        private Usuario? _aluno;

        public PagamentoViewModel()
        {
            Title = "Finalizar compra";
            ConfirmarCommand = new Command(async () => await ConfirmarAsync());
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public Curso? Curso
        {
            get => _curso;
            set
            {
                if (SetProperty(ref _curso, value))
                {
                    OnPropertyChanged(nameof(CursoNome));
                    OnPropertyChanged(nameof(CursoDescricao));
                    OnPropertyChanged(nameof(CursoCategoria));
                    OnPropertyChanged(nameof(Valor));
                    OnPropertyChanged(nameof(CargaHoraria));
                    OnPropertyChanged(nameof(TotalAulas));
                    OnPropertyChanged(nameof(Coordenador));
                    OnPropertyChanged(nameof(DocentesTexto));
                    OnPropertyChanged(nameof(TemCorpoDocente));
                }
            }
        }

        public string CursoNome => Curso?.Nome ?? string.Empty;
        public string CursoDescricao => Curso?.Descricao ?? string.Empty;
        public string CursoCategoria => $"Categoria: {Curso?.Categoria}";
        public string Valor => Curso == null ? string.Empty : Curso.Preco.ToString("C");
        public string CargaHoraria => Curso == null ? string.Empty : $"{Curso.CargaHorariaHoras} horas";
        public int TotalAulas => Curso?.Modulos.Sum(m => m.Materias.Sum(x => x.Aulas.Count)) ?? 0;
        public string Coordenador => Curso?.Coordenador ?? string.Empty;
        public string DocentesTexto => Curso == null || Curso.Docentes.Count == 0
            ? string.Empty
            : string.Join("\n", Curso.Docentes);
        public bool TemCoordenador => !string.IsNullOrWhiteSpace(Curso?.Coordenador);
        public bool TemCorpoDocente => TemCoordenador || (Curso?.Docentes.Count ?? 0) > 0;

        public ICommand ConfirmarCommand { get; }
        public ICommand VoltarCommand { get; }

        public void Carregar(Curso curso)
        {
            _aluno = SessionService.UsuarioAtual;
            Curso = curso;
            Title = curso.Nome;
        }

        private async Task ConfirmarAsync()
        {
            if (Curso is null)
                return;

            var aluno = SessionService.UsuarioAtual;

            if (aluno is null && SessionService.CadastroPendente is { } dados)
            {
                aluno = new Usuario
                {
                    Id = Guid.NewGuid().ToString(),
                    Nome = dados.Nome,
                    Email = dados.Email,
                    SenhaHash = SenhaService.GerarHash(dados.Senha),
                    Roles = Enums.Roles.Aluno
                };

                MockDataService.AdicionarUsuario(aluno);
                SessionService.Entrar(aluno);
                SessionService.LimparCadastroPendente();
            }

            if (aluno is null)
            {
                await Shell.Current.GoToAsync("cadastro");
                return;
            }

            MockDataService.RegistrarMatricula(aluno, Curso);

            await Shell.Current.DisplayAlert(
                "Pagamento confirmado",
                "Assinatura confirmada (simulada). Agora você já pode acessar as aulas do curso.",
                "Continuar");

            await Shell.Current.GoToAsync("//main");
        }
    }
}