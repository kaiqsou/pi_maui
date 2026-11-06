using System.Windows.Input;
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
    /// Gestão do conteúdo de um curso. A coordenação edita a estrutura (curso, módulos, matérias) e as aulas;
    /// os professores do curso editam aulas, materiais e as próprias provas.
    /// </summary>
    public class CursoDetalheViewModel : BaseViewModel
    {
        // uma página por módulo, como na aba Aulas do aluno
        private const int ModulosPorPagina = 1;

        private Curso? _curso;
        private string _professoresTexto = string.Empty;
        private List<ModuloNode> _todosModulos = new();
        private int _indicePagina;
        private IReadOnlyList<ModuloNode> _modulosPagina = new List<ModuloNode>();

        public CursoDetalheViewModel()
        {
            VoltarCommand = new Command(async () => await VoltarAsync());
            EditarCursoCommand = new Command(async () => await IrAsync("cursoedit", new() { ["Curso"] = Curso! }));
            NovoModuloCommand = new Command(async () => await IrAsync("moduloedit", new() { ["Curso"] = Curso! }));
            EditarModuloCommand = new Command<Modulo>(async m => await IrAsync("moduloedit", new() { ["Curso"] = Curso!, ["Modulo"] = m }));
            ExcluirModuloCommand = new Command<Modulo>(async m => await ExcluirModuloAsync(m));
            NovaMateriaCommand = new Command<Modulo>(async m => await IrAsync("materiaedit", new() { ["Modulo"] = m }));
            EditarMateriaCommand = new Command<Materia>(async m => await IrAsync("materiaedit", new() { ["Modulo"] = m.Modulo!, ["Materia"] = m }));
            ExcluirMateriaCommand = new Command<Materia>(async m => await ExcluirMateriaAsync(m));
            NovaAulaCommand = new Command<Materia>(async m => await IrAsync("aulaedit", new() { ["Materia"] = m }));
            EditarAulaCommand = new Command<Aula>(async a => await IrAsync("aulaedit", new() { ["Materia"] = a.Materia!, ["Aula"] = a }));
            ExcluirAulaCommand = new Command<Aula>(async a => await ExcluirAulaAsync(a));
            NovaProvaCommand = new Command<Materia>(async m => await IrAsync("provaedit", new() { ["Materia"] = m }));
            EditarProvaCommand = new Command<ProvaNode>(async p => await IrAsync("provaedit", new() { ["Prova"] = p.Prova }));
            PaginaAnteriorCommand = new Command(() => MudarPagina(-1));
            PaginaProximaCommand = new Command(() => MudarPagina(1));

            // menus "⋮" e toque nos itens: concentram as ações e deixam a tela limpa
            OpcoesCursoCommand = new Command(async () => await OpcoesCursoAsync());
            OpcoesModuloCommand = new Command(async () => await OpcoesModuloAsync());
            OpcoesMateriaCommand = new Command<MateriaNode>(async m => await OpcoesMateriaAsync(m));
            OpcoesAulaCommand = new Command<Aula>(async a => await OpcoesAulaAsync(a));
            AbrirProvaCommand = new Command<ProvaNode>(async p => { if (p?.PodeEditar == true) await IrAsync("provaedit", new() { ["Prova"] = p.Prova }); });
        }

        public ICommand OpcoesCursoCommand { get; }
        public ICommand OpcoesModuloCommand { get; }
        public ICommand OpcoesMateriaCommand { get; }
        public ICommand OpcoesAulaCommand { get; }
        public ICommand AbrirProvaCommand { get; }

        /// <summary>Módulo exibido na página atual.</summary>
        public ModuloNode? ModuloAtual => ModulosPagina.FirstOrDefault();
        public string ModuloAtualTitulo => ModuloAtual?.Modulo.Titulo ?? string.Empty;
        public bool TemModulos => _todosModulos.Count > 0;
        public bool SemModulos => !TemModulos;
        public bool ModuloSemMaterias => ModuloAtual != null && ModuloAtual.Materias.Count == 0;
        public bool TemOpcoesModulo => PodeEditarEstrutura && ModuloAtual != null;

        public Curso? Curso
        {
            get => _curso;
            set
            {
                if (SetProperty(ref _curso, value))
                {
                    OnPropertyChanged(nameof(CargaHorariaFormatada));
                    OnPropertyChanged(nameof(ResumoTexto));
                }
            }
        }

        public IReadOnlyList<ModuloNode> ModulosPagina
        {
            get => _modulosPagina;
            private set => SetProperty(ref _modulosPagina, value);
        }

        public int TotalPaginas => _todosModulos.Count == 0 ? 1 : (_todosModulos.Count + ModulosPorPagina - 1) / ModulosPorPagina;
        public int PaginaAtual => _indicePagina + 1;
        public bool TemPaginacao => TotalPaginas > 1;
        public bool PodePaginaAnterior => _indicePagina > 0;
        public bool PodePaginaProxima => _indicePagina < TotalPaginas - 1;
        public string IndicadorPagina => $"{PaginaAtual} / {TotalPaginas}";

        public string CargaHorariaFormatada => Curso == null ? string.Empty : $"{Curso.CargaHorariaHoras} horas";
        public string ResumoTexto => Curso == null ? string.Empty : $"{Curso.Modulos.Count} módulos · {Curso.TotalMaterias} matérias · {Curso.TotalAulas} aulas";

        public string ProfessoresTexto
        {
            get => _professoresTexto;
            private set => SetProperty(ref _professoresTexto, value);
        }

        private static Usuario? Usuario => SessionService.UsuarioAtual;

        public bool PodeEditarEstrutura => Usuario?.IsCoordenacao == true;
        public bool IsProfessorDoCurso => Usuario?.Roles == Roles.Professor && Curso?.ProfessorIds.Contains(Usuario.Id) == true;
        public bool PodeEditarAulas => PodeEditarEstrutura || IsProfessorDoCurso;
        public bool PodeCriarProvas => IsProfessorDoCurso;

        public ICommand VoltarCommand { get; }
        public ICommand EditarCursoCommand { get; }
        public ICommand NovoModuloCommand { get; }
        public ICommand EditarModuloCommand { get; }
        public ICommand ExcluirModuloCommand { get; }
        public ICommand NovaMateriaCommand { get; }
        public ICommand EditarMateriaCommand { get; }
        public ICommand ExcluirMateriaCommand { get; }
        public ICommand NovaAulaCommand { get; }
        public ICommand EditarAulaCommand { get; }
        public ICommand ExcluirAulaCommand { get; }
        public ICommand NovaProvaCommand { get; }
        public ICommand EditarProvaCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PaginaProximaCommand { get; }

        public Task CarregarAsync(string cursoId) => ExecutarAsync(async () =>
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
            MontarModulos(provas);

            OnPropertyChanged(nameof(PodeEditarEstrutura));
            OnPropertyChanged(nameof(PodeEditarAulas));
            OnPropertyChanged(nameof(PodeCriarProvas));
            OnPropertyChanged(nameof(TemOpcoesModulo));
        });

        private void MontarModulos(List<Prova> provas)
        {
            var paginaAnterior = _indicePagina;
            _todosModulos = new List<ModuloNode>();

            foreach (var modulo in Curso!.Modulos.OrderBy(m => m.Ordem))
            {
                var node = new ModuloNode { Modulo = modulo };

                foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                {
                    node.Materias.Add(new MateriaNode
                    {
                        Materia = materia,
                        Aulas = materia.Aulas.OrderBy(a => a.Ordem).ToList(),
                        Provas = provas.Where(p => p.MateriaId == materia.Id)
                            .Select(p => new ProvaNode { Prova = p, PodeEditar = p.ProfessorId == Usuario?.Id })
                            .ToList()
                    });
                }

                _todosModulos.Add(node);
            }

            _indicePagina = Math.Min(paginaAnterior, TotalPaginas - 1);
            AplicarPagina();
            OnPropertyChanged(nameof(ResumoTexto));
        }

        private void MudarPagina(int delta)
        {
            var novo = _indicePagina + delta;
            if (novo < 0 || novo >= TotalPaginas)
                return;

            _indicePagina = novo;
            AplicarPagina();
        }

        private void AplicarPagina()
        {
            ModulosPagina = _todosModulos
                .Skip(_indicePagina * ModulosPorPagina)
                .Take(ModulosPorPagina)
                .ToList();

            OnPropertyChanged(nameof(PaginaAtual));
            OnPropertyChanged(nameof(TotalPaginas));
            OnPropertyChanged(nameof(TemPaginacao));
            OnPropertyChanged(nameof(PodePaginaAnterior));
            OnPropertyChanged(nameof(PodePaginaProxima));
            OnPropertyChanged(nameof(IndicadorPagina));
            OnPropertyChanged(nameof(ModuloAtual));
            OnPropertyChanged(nameof(ModuloAtualTitulo));
            OnPropertyChanged(nameof(TemModulos));
            OnPropertyChanged(nameof(SemModulos));
            OnPropertyChanged(nameof(ModuloSemMaterias));
            OnPropertyChanged(nameof(TemOpcoesModulo));
        }

        private async Task OpcoesCursoAsync()
        {
            if (Curso == null || !PodeEditarEstrutura)
                return;

            var escolha = await Shell.Current.DisplayActionSheet(Curso.Nome, "Cancelar", "Excluir curso", "Editar curso");
            if (escolha == "Editar curso")
            {
                EditarCursoCommand.Execute(null);
                return;
            }

            if (escolha != "Excluir curso" ||
                !await ConfirmarAsync($"Excluir \"{Curso.Nome}\"?\n\nMatrículas, progresso, provas, notas e materiais do curso serão apagados. Certificados já emitidos são mantidos."))
                return;

            if (await ExecutarAsync(() => CursoRepositorio.ExcluirAsync(Curso.Id)))
                await VoltarAsync();
        }

        private async Task OpcoesModuloAsync()
        {
            var modulo = ModuloAtual?.Modulo;
            if (modulo == null || !PodeEditarEstrutura)
                return;

            var escolha = await Shell.Current.DisplayActionSheet(modulo.Titulo, "Cancelar", "Excluir módulo", "Nova matéria", "Editar módulo");
            switch (escolha)
            {
                case "Nova matéria": NovaMateriaCommand.Execute(modulo); break;
                case "Editar módulo": EditarModuloCommand.Execute(modulo); break;
                case "Excluir módulo": await ExcluirModuloAsync(modulo); break;
            }
        }

        private async Task OpcoesMateriaAsync(MateriaNode? node)
        {
            if (node == null)
                return;

            var opcoes = new List<string>();
            if (PodeEditarAulas) opcoes.Add("Nova aula");
            if (PodeCriarProvas) opcoes.Add("Nova prova");
            if (PodeEditarEstrutura) opcoes.Add("Editar matéria");
            if (opcoes.Count == 0 && !PodeEditarEstrutura)
                return;

            var escolha = await Shell.Current.DisplayActionSheet(node.Materia.Nome, "Cancelar",
                PodeEditarEstrutura ? "Excluir matéria" : null, opcoes.ToArray());
            switch (escolha)
            {
                case "Nova aula": NovaAulaCommand.Execute(node.Materia); break;
                case "Nova prova": NovaProvaCommand.Execute(node.Materia); break;
                case "Editar matéria": EditarMateriaCommand.Execute(node.Materia); break;
                case "Excluir matéria": await ExcluirMateriaAsync(node.Materia); break;
            }
        }

        private async Task OpcoesAulaAsync(Aula? aula)
        {
            if (aula == null || !PodeEditarAulas)
                return;

            var escolha = await Shell.Current.DisplayActionSheet(aula.Titulo, "Cancelar", "Excluir aula", "Editar aula");
            if (escolha == "Editar aula")
                EditarAulaCommand.Execute(aula);
            else if (escolha == "Excluir aula")
                await ExcluirAulaAsync(aula);
        }

        private static Task IrAsync(string rota, Dictionary<string, object> parametros)
            => Shell.Current.GoToAsync(rota, parametros);

        private async Task ExcluirModuloAsync(Modulo? modulo)
        {
            if (modulo == null || Curso == null)
                return;

            if (!await ConfirmarAsync($"Excluir o módulo \"{modulo.Titulo}\" com todas as matérias, aulas e provas?"))
                return;

            await ExecutarAsync(() => CursoRepositorio.ExcluirModuloAsync(Curso.Id, modulo));
            await CarregarAsync(Curso.Id);
        }

        private async Task ExcluirMateriaAsync(Materia? materia)
        {
            if (materia == null || Curso == null)
                return;

            if (!await ConfirmarAsync($"Excluir a matéria \"{materia.Nome}\" com suas aulas e provas?"))
                return;

            await ExecutarAsync(() => CursoRepositorio.ExcluirMateriaAsync(Curso.Id, materia));
            await CarregarAsync(Curso.Id);
        }

        private async Task ExcluirAulaAsync(Aula? aula)
        {
            if (aula == null || Curso == null)
                return;

            if (!await ConfirmarAsync($"Excluir a aula \"{aula.Titulo}\" e seus materiais?"))
                return;

            await ExecutarAsync(() => CursoRepositorio.ExcluirAulaAsync(Curso.Id, aula));
            await CarregarAsync(Curso.Id);
        }

    }
}
