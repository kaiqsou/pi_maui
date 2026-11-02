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

    public class TurmaResumo
    {
        public string Nome { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
    }

    public class ProvaDoProfessor
    {
        public Prova Prova { get; init; } = new();
        public string Titulo => Prova.Titulo;
        public string Detalhe { get; init; } = string.Empty;
        public bool Publicada => Prova.IsPublicada;
        public string StatusTexto => Prova.IsPublicada ? "Publicada" : "Rascunho";
    }

    /// <summary>Coordenação: cursos, turmas e provas de um professor e o acesso dele ao aplicativo.</summary>
    public class ProfessorAcessoViewModel : BaseViewModel
    {
        private Usuario? _professor;
        private string _resumoTexto = string.Empty;

        public ProfessorAcessoViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
            AlternarAcessoCommand = new Command(async () => await AlternarAcessoAsync());
            RedefinirSenhaCommand = new Command(async () => await RedefinirSenhaAsync());
            AbrirProvaCommand = new Command<ProvaDoProfessor>(async p =>
            {
                if (p != null)
                    await Shell.Current.GoToAsync("provaresultados", new Dictionary<string, object> { ["Prova"] = p.Prova });
            });
        }

        public ObservableCollection<TurmaResumo> Turmas { get; } = new();
        public ObservableCollection<ProvaDoProfessor> Provas { get; } = new();
        public bool TemTurmas => Turmas.Count > 0;
        public bool TemProvas => Provas.Count > 0;

        /// <summary>"2 cursos · 3 provas · 1 turma", no cartão do professor.</summary>
        public string ResumoTexto
        {
            get => _resumoTexto;
            private set => SetProperty(ref _resumoTexto, value);
        }

        public ICommand AbrirProvaCommand { get; }

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

            var cursos = await CursoRepositorio.ListarAsync();
            Cursos.Clear();
            foreach (var curso in cursos)
            {
                var atribuido = curso.ProfessorIds.Contains(Professor.Id);
                Cursos.Add(new CursoAtribuicao { Curso = curso, Original = atribuido, Atribuido = atribuido });
            }

            var nomesCursos = cursos.ToDictionary(c => c.Id, c => c.Nome);

            // turmas (salas) que o professor criou
            var salas = await SalaRepositorio.DoProfessorAsync(Professor.Id);
            Turmas.Clear();
            foreach (var sala in salas)
            {
                var curso = sala.CursoId == null ? "Todos os cursos" : nomesCursos.GetValueOrDefault(sala.CursoId, "Curso removido");
                Turmas.Add(new TurmaResumo
                {
                    Nome = sala.Nome,
                    Detalhe = $"{Plural(sala.AlunoIds.Count, "aluno", "alunos")} · {curso}"
                });
            }

            // provas que o professor criou, com quantos alunos já fizeram
            var provas = await ProvaRepositorio.DoProfessorAsync(Professor.Id);
            var respostas = (await ResultadoRepositorio.DasProvasAsync(provas.Select(p => p.Id)))
                .GroupBy(r => r.ProvaId).ToDictionary(g => g.Key, g => g.Count());
            Provas.Clear();
            foreach (var prova in provas)
            {
                var curso = cursos.FirstOrDefault(c => c.Id == prova.CursoId);
                var materia = curso?.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId)?.Nome;
                Provas.Add(new ProvaDoProfessor
                {
                    Prova = prova,
                    Detalhe = $"{materia ?? curso?.Nome ?? "Curso removido"} · {Plural(respostas.GetValueOrDefault(prova.Id), "resposta", "respostas")}"
                });
            }

            ResumoTexto = $"{Plural(cursos.Count(c => c.ProfessorIds.Contains(Professor.Id)), "curso", "cursos")} · " +
                          $"{Plural(provas.Count, "prova", "provas")} · {Plural(salas.Count, "turma", "turmas")}";
            OnPropertyChanged(nameof(TemTurmas));
            OnPropertyChanged(nameof(TemProvas));
        });

        private static string Plural(int n, string um, string varios) => $"{n} {(n == 1 ? um : varios)}";

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
                await CarregarAsync(Professor.Id);
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

            var hash = SenhaService.GerarHash(senha);
            if (await ExecutarAsync(() => UsuarioRepositorio.AtualizarSenhaAsync(Professor.Id, hash, provisoria: true)))
                await AvisarAsync("Senha", "Senha alterada. Informe a nova senha ao professor.");
        }
    }
}
