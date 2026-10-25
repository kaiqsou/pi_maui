using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CursoAtribuicao : BaseViewModel
    {
        private bool _atribuido;

        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;
        public bool Original { get; init; }
        public bool Atribuido { get => _atribuido; set => SetProperty(ref _atribuido, value); }
    }

    /// <summary>Coordenação: cursos de um professor e acesso dele ao aplicativo.</summary>
    public class ProfessorAcessoViewModel : BaseViewModel
    {
        private Usuario? _professor;

        public ProfessorAcessoViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
            AlternarAcessoCommand = new Command(async () => await AlternarAcessoAsync());
            RedefinirSenhaCommand = new Command(async () => await RedefinirSenhaAsync());
        }

        public Usuario? Professor
        {
            get => _professor;
            private set
            {
                if (SetProperty(ref _professor, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Iniciais));
                    OnPropertyChanged(nameof(AcessoLiberado));
                    OnPropertyChanged(nameof(StatusTexto));
                    OnPropertyChanged(nameof(AcessoBotaoTexto));
                }
            }
        }

        public string Nome => Professor?.Nome ?? string.Empty;
        public string Email => Professor?.Email ?? string.Empty;
        public string Iniciais => Professor?.Iniciais ?? "?";
        public bool AcessoLiberado => Professor?.AcessoLiberado == true;
        public string StatusTexto => AcessoLiberado ? "Acesso liberado" : "Sem acesso ao aplicativo";
        public string AcessoBotaoTexto => AcessoLiberado ? "Remover acesso ao aplicativo" : "Restaurar acesso";

        public ObservableCollection<CursoAtribuicao> Cursos { get; } = new();

        public ICommand SalvarCommand { get; }
        public ICommand AlternarAcessoCommand { get; }
        public ICommand RedefinirSenhaCommand { get; }

        public Task CarregarAsync(string professorId) => ExecutarAsync(async () =>
        {
            Professor = await UsuarioRepositorio.PorIdAsync(professorId);
            if (Professor == null)
                return;

            Cursos.Clear();
            foreach (var curso in await CursoRepositorio.ListarAsync())
            {
                var atribuido = curso.ProfessorIds.Contains(Professor.Id);
                Cursos.Add(new CursoAtribuicao { Curso = curso, Original = atribuido, Atribuido = atribuido });
            }
        });

        private async Task SalvarAsync()
        {
            if (Professor == null)
                return;

            var alterados = Cursos.Where(c => c.Atribuido != c.Original).ToList();
            var ok = await ExecutarAsync(async () =>
            {
                foreach (var item in alterados)
                {
                    await CursoRepositorio.AlterarAsync(item.Curso.Id, curso =>
                    {
                        curso.ProfessorIds.Remove(Professor.Id);
                        if (item.Atribuido)
                            curso.ProfessorIds.Add(Professor.Id);
                    });
                }
            });

            if (ok)
            {
                await AvisarAsync("Professor", alterados.Count == 0 ? "Nenhuma alteração nos cursos." : "Cursos do professor atualizados.");
                await VoltarAsync();
            }
        }

        private async Task AlternarAcessoAsync()
        {
            if (Professor == null)
                return;

            var remover = AcessoLiberado;
            if (remover && !await ConfirmarAsync($"Remover o acesso de {Professor.Nome}? As provas e aulas criadas por ele continuam no curso.", "Remover acesso"))
                return;

            Professor.StatusAcesso = remover ? StatusAcesso.Removido : StatusAcesso.Liberado;
            if (await ExecutarAsync(() => UsuarioRepositorio.AtualizarAsync(Professor)))
            {
                OnPropertyChanged(nameof(AcessoLiberado));
                OnPropertyChanged(nameof(StatusTexto));
                OnPropertyChanged(nameof(AcessoBotaoTexto));
            }
        }

        private async Task RedefinirSenhaAsync()
        {
            if (Professor == null)
                return;

            var senha = await Shell.Current.DisplayPromptAsync("Nova senha", $"Defina uma senha provisória para {Professor.Nome}.", "Salvar", "Cancelar");
            if (string.IsNullOrEmpty(senha))
                return;

            if (!SenhaService.EhValida(senha, out var erro))
            {
                await AvisarAsync("Senha", erro);
                return;
            }

            Professor.SenhaHash = SenhaService.GerarHash(senha);
            if (await ExecutarAsync(() => UsuarioRepositorio.AtualizarAsync(Professor)))
                await AvisarAsync("Senha", "Senha alterada. Informe a nova senha ao professor.");
        }
    }
}
