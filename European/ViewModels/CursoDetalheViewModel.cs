using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ProvaNode
    {
        public Prova Prova { get; set; } = new();
        public bool PodeEditar { get; set; }
        public string Titulo => Prova.Titulo;
        public string Resumo => $"{Prova.Questoes.Count} questões · {(Prova.IsPublicada ? "Publicada" : "Rascunho")}";
    }

    public class MateriaNode
    {
        public Materia Materia { get; set; } = new();
        public List<Aula> Aulas { get; set; } = new();
        public List<ProvaNode> Provas { get; set; } = new();
        public string Resumo => $"{Aulas.Count} aula(s) · {Provas.Count} prova(s)";
        public bool SemConteudo => Aulas.Count == 0 && Provas.Count == 0;
    }

    public class ModuloNode
    {
        public Modulo Modulo { get; set; } = new();
        public List<MateriaNode> Materias { get; set; } = new();
    }

    /// <summary>
    /// Gestão do conteúdo de um curso, um módulo por página. A coordenação edita a estrutura (curso, módulos,
    /// matérias) e as aulas; os professores do curso editam aulas, materiais e as próprias provas.
    /// As ações ficam nos menus "⋮" e no toque em cada item.
    /// </summary>
    public partial class CursoDetalheViewModel : BaseViewModel
    {
        private string? _cursoId;
        private List<ModuloNode> _modulos = new();

        public CursoDetalheViewModel()
        {
            Paginas.Mudou += AoMudarPagina;
        }

        public Paginador Paginas { get; } = new();

        [ObservableProperty]
        public partial Curso? Curso { get; private set; }

        [ObservableProperty]
        public partial string ProfessoresTexto { get; private set; } = string.Empty;

        /// <summary>Módulo exibido na página atual.</summary>
        public ModuloNode? ModuloAtual => Paginas.Indice < _modulos.Count ? _modulos[Paginas.Indice] : null;
        public string ModuloAtualTitulo => ModuloAtual?.Modulo.Titulo ?? string.Empty;
        public bool TemModulos => _modulos.Count > 0;
        public bool SemModulos => !TemModulos;
        public bool ModuloSemMaterias => ModuloAtual is { Materias.Count: 0 };
        public bool TemOpcoesModulo => PodeEditarEstrutura && ModuloAtual != null;

        private static Usuario? Usuario => SessionService.UsuarioAtual;

        public bool PodeEditarEstrutura => Usuario?.IsCoordenacao == true;
        public bool IsProfessorDoCurso => Usuario?.Roles == Roles.Professor && Curso?.ProfessorIds.Contains(Usuario.Id) == true;
        public bool PodeEditarAulas => PodeEditarEstrutura || IsProfessorDoCurso;
        public bool PodeCriarProvas => IsProfessorDoCurso;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _cursoId = parametros.Obter<Curso>("Curso")?.Id ?? _cursoId;
            return Task.CompletedTask;
        }

        /// <summary>Recarrega sempre que a tela aparece (ex.: ao voltar da edição de uma aula), mantendo a página.</summary>
        public override Task AoAparecerAsync() => _cursoId == null ? Task.CompletedTask : CarregarAsync(_cursoId);

        private Task CarregarAsync(string cursoId) => ExecutarAsync(async () =>
        {
            var curso = await CursoRepositorio.PorIdAsync(cursoId);
            if (curso == null)
            {
                await VoltarAsync();
                return;
            }

            Curso = curso;
            Title = curso.Nome;

            var professores = await UsuarioRepositorio.PorIdsAsync(curso.ProfessorIds);
            ProfessoresTexto = professores.Count == 0 ? "Nenhum professor atribuído" : string.Join(", ", professores.Select(p => p.Nome));

            var provas = await ProvaRepositorio.DosCursosAsync(new[] { curso.Id });
            _modulos = curso.Modulos.OrderBy(m => m.Ordem).Select(modulo => new ModuloNode
            {
                Modulo = modulo,
                Materias = modulo.Materias.OrderBy(m => m.Ordem).Select(materia => new MateriaNode
                {
                    Materia = materia,
                    Aulas = materia.Aulas.OrderBy(a => a.Ordem).ToList(),
                    Provas = provas.Where(p => p.MateriaId == materia.Id)
                        .Select(p => new ProvaNode { Prova = p, PodeEditar = p.ProfessorId == Usuario?.Id })
                        .ToList()
                }).ToList()
            }).ToList();

            OnPropertyChanged(nameof(PodeEditarEstrutura));
            OnPropertyChanged(nameof(PodeEditarAulas));
            OnPropertyChanged(nameof(PodeCriarProvas));
            Paginas.Reiniciar(_modulos.Count, Paginas.Indice);
        });

        private void AoMudarPagina()
        {
            OnPropertyChanged(nameof(ModuloAtual));
            OnPropertyChanged(nameof(ModuloAtualTitulo));
            OnPropertyChanged(nameof(TemModulos));
            OnPropertyChanged(nameof(SemModulos));
            OnPropertyChanged(nameof(ModuloSemMaterias));
            OnPropertyChanged(nameof(TemOpcoesModulo));
        }

        // ---- Comandos da tela ----


        [RelayCommand]
        private Task NovoModulo()
            => Curso == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ModuloEdit, ("Curso", Curso));

        [RelayCommand]
        private Task AbrirProva(ProvaNode? node)
            => node?.PodeEditar == true ? Navegacao.IrAsync(Navegacao.ProvaEdit, ("Prova", node.Prova)) : Task.CompletedTask;

        [RelayCommand]
        private async Task OpcoesCursoAsync()
        {
            if (Curso == null || !PodeEditarEstrutura)
                return;

            switch (await EscolherOpcaoAsync(Curso.Nome, "Excluir curso", "Editar curso"))
            {
                case "Editar curso":
                    await Navegacao.IrAsync(Navegacao.CursoEdit, ("Curso", Curso));
                    break;
                case "Excluir curso":
                    await ExcluirEVoltarAsync(
                        $"Excluir \"{Curso.Nome}\"?\n\nMatrículas, progresso, provas, notas e materiais do curso serão apagados. Certificados já emitidos são mantidos.",
                        () => CursoRepositorio.ExcluirAsync(Curso.Id));
                    break;
            }
        }

        [RelayCommand]
        private async Task OpcoesModuloAsync()
        {
            var modulo = ModuloAtual?.Modulo;
            if (modulo == null || Curso == null || !PodeEditarEstrutura)
                return;

            switch (await EscolherOpcaoAsync(modulo.Titulo, "Excluir módulo", "Nova matéria", "Editar módulo"))
            {
                case "Nova matéria":
                    await Navegacao.IrAsync(Navegacao.MateriaEdit, ("Modulo", modulo));
                    break;
                case "Editar módulo":
                    await Navegacao.IrAsync(Navegacao.ModuloEdit, ("Curso", Curso), ("Modulo", modulo));
                    break;
                case "Excluir módulo":
                    await ExcluirEAtualizarAsync($"Excluir o módulo \"{modulo.Titulo}\" com todas as matérias, aulas e provas?",
                        () => CursoRepositorio.ExcluirModuloAsync(Curso.Id, modulo));
                    break;
            }
        }

        [RelayCommand]
        private async Task OpcoesMateriaAsync(MateriaNode? node)
        {
            if (node == null || Curso == null)
                return;

            var materia = node.Materia;
            var opcoes = new List<string>();
            if (PodeEditarAulas) opcoes.Add("Nova aula");
            if (PodeCriarProvas) opcoes.Add("Nova prova");
            if (PodeEditarEstrutura) opcoes.Add("Editar matéria");
            if (opcoes.Count == 0)
                return;

            switch (await EscolherOpcaoAsync(materia.Nome, PodeEditarEstrutura ? "Excluir matéria" : null, opcoes.ToArray()))
            {
                case "Nova aula":
                    await Navegacao.IrAsync(Navegacao.AulaEdit, ("Materia", materia));
                    break;
                case "Nova prova":
                    await Navegacao.IrAsync(Navegacao.ProvaEdit, ("Materia", materia));
                    break;
                case "Editar matéria":
                    await Navegacao.IrAsync(Navegacao.MateriaEdit, ("Modulo", materia.Modulo), ("Materia", materia));
                    break;
                case "Excluir matéria":
                    await ExcluirEAtualizarAsync($"Excluir a matéria \"{materia.Nome}\" com suas aulas e provas?",
                        () => CursoRepositorio.ExcluirMateriaAsync(Curso.Id, materia));
                    break;
            }
        }

        [RelayCommand]
        private async Task OpcoesAulaAsync(Aula? aula)
        {
            if (aula == null || Curso == null || !PodeEditarAulas)
                return;

            switch (await EscolherOpcaoAsync(aula.Titulo, "Excluir aula", "Editar aula"))
            {
                case "Editar aula":
                    await Navegacao.IrAsync(Navegacao.AulaEdit, ("Materia", aula.Materia), ("Aula", aula));
                    break;
                case "Excluir aula":
                    await ExcluirEAtualizarAsync($"Excluir a aula \"{aula.Titulo}\" e seus materiais?",
                        () => CursoRepositorio.ExcluirAulaAsync(Curso.Id, aula));
                    break;
            }
        }

        private async Task ExcluirEAtualizarAsync(string pergunta, Func<Task> excluir)
        {
            if (!await ConfirmarAsync(pergunta))
                return;

            await ExecutarAsync(excluir);
            await AoAparecerAsync();
        }
    }
}
