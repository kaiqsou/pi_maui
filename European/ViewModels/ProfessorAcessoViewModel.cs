using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class CursoAtribuicao : ObservableObject
    {
        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;

        /// <summary>Se o curso já era do professor ao abrir a tela (para salvar só o que mudou).</summary>
        public bool Original { get; init; }

        [ObservableProperty]
        public partial bool Atribuido { get; set; }
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
    public partial class ProfessorAcessoViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Email), nameof(Iniciais), nameof(AcessoLiberado), nameof(StatusTexto), nameof(AcessoBotaoTexto))]
        public partial Usuario? Professor { get; private set; }

        public string Nome => Professor?.Nome ?? string.Empty;
        public string Email => Professor?.Email ?? string.Empty;
        public string Iniciais => Professor?.Iniciais ?? "?";
        public bool AcessoLiberado => Professor?.AcessoLiberado == true;
        public string StatusTexto => AcessoLiberado ? "Acesso liberado" : "Sem acesso ao aplicativo";
        public string AcessoBotaoTexto => AcessoLiberado ? "Remover acesso ao aplicativo" : "Restaurar acesso";

        public ObservableCollection<CursoAtribuicao> Cursos { get; } = new();
        public ObservableCollection<TurmaResumo> Turmas { get; } = new();
        public ObservableCollection<ProvaDoProfessor> Provas { get; } = new();
        public bool TemTurmas => Turmas.Count > 0;
        public bool TemProvas => Provas.Count > 0;

        /// <summary>"2 cursos · 3 provas · 1 turma", no cartão do professor.</summary>
        [ObservableProperty]
        public partial string ResumoTexto { get; private set; } = string.Empty;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            var professor = parametros.Obter<Usuario>("Usuario");
            parametros.Clear();
            return professor == null ? Task.CompletedTask : CarregarAsync(professor.Id);
        }

        private Task CarregarAsync(string professorId) => ExecutarAsync(async () =>
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
                Turmas.Add(new TurmaResumo { Nome = sala.Nome, Detalhe = $"{Plural(sala.AlunoIds.Count, "aluno", "alunos")} · {curso}" });
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

        /// <summary>Grava só os cursos que foram marcados ou desmarcados.</summary>
        [RelayCommand]
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
                var novos = alterados.Where(c => c.Atribuido).Select(c => c.Nome).ToList();
                if (novos.Count > 0)
                    await NotificacaoService.ParaUsuarioAsync(Professor.Id, "Novo curso",
                        $"A coordenação atribuiu a você: {string.Join(", ", novos)}.");

                await AvisarAsync("Professor", alterados.Count == 0 ? "Nenhuma alteração nos cursos." : "Cursos do professor atualizados.");
                await CarregarAsync(Professor.Id);
            }
        }

        [RelayCommand]
        private async Task AlternarAcessoAsync()
        {
            if (Professor == null)
                return;

            var remover = AcessoLiberado;
            if (remover && !await ConfirmarAsync($"Remover o acesso de {Professor.Nome}? As provas e aulas criadas por ele continuam no curso.", "Remover acesso"))
                return;

            if (await ExecutarAsync(() => ContaService.DefinirAcessoProfessorAsync(Professor, liberado: !remover)))
            {
                OnPropertyChanged(nameof(AcessoLiberado));
                OnPropertyChanged(nameof(StatusTexto));
                OnPropertyChanged(nameof(AcessoBotaoTexto));
            }
        }

        [RelayCommand]
        private Task RedefinirSenha() => Professor == null ? Task.CompletedTask : RedefinirSenhaProvisoriaAsync(Professor, "professor");

        [RelayCommand]
        private Task AbrirProva(ProvaDoProfessor? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ProvaResultados, ("Prova", item.Prova));
    }
}
