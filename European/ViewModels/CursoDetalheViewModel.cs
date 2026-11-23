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
    /// Gestão do conteúdo de um curso, um módulo por vez (escolhido no seletor de módulo). A coordenação edita a estrutura (curso, módulos,
    /// matérias) e as aulas; os professores do curso editam aulas, materiais e as próprias provas.
    /// As ações ficam nos menus "⋮" e no toque em cada item.
    /// </summary>
    public partial class CursoDetalheViewModel : BaseViewModel
    {
        private string? _cursoId;
        private List<ModuloNode> _modulos = new();

        /// <summary>Módulos do curso para o seletor (só aparece com mais de um módulo).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemVariosModulos), nameof(TemUmModulo))]
        public partial List<ModuloOpcao> Modulos { get; private set; } = new();

        public bool TemVariosModulos => Modulos.Count > 1;
        public bool TemUmModulo => Modulos.Count == 1;

        [ObservableProperty]
        public partial ModuloOpcao? ModuloSelecionado { get; set; }

        partial void OnModuloSelecionadoChanged(ModuloOpcao? value) => AoMudarModulo();

        [ObservableProperty]
        public partial Curso? Curso { get; private set; }

        [ObservableProperty]
        public partial string ProfessoresTexto { get; private set; } = string.Empty;

        /// <summary>Módulo escolhido no seletor.</summary>
        public ModuloNode? ModuloAtual => _modulos.FirstOrDefault(n => n.Modulo.Id == ModuloSelecionado?.Valor?.Id);
        public string ModuloAtualTitulo => ModuloAtual?.Modulo.Titulo ?? string.Empty;
        public bool TemModulos => _modulos.Count > 0;
        public bool SemModulos => !TemModulos;
        public bool ModuloSemMaterias => ModuloAtual is { Materias.Count: 0 };
        public bool TemOpcoesModulo => PodeEditarEstrutura && ModuloAtual != null;

        private static Usuario? Usuario => SessionService.UsuarioAtual;

        public bool IsCoordenacao => Usuario?.IsCoordenacao == true;
        public bool IsProfessorDoCurso => Usuario?.Roles == Roles.Professor && Curso?.ProfessorIds.Contains(Usuario.Id) == true;

        /// <summary>A tela abre só para visualizar; o botão "Editar" liga os menus de edição.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PodeEditarEstrutura), nameof(PodeEditarAulas), nameof(PodeCriarProvas),
            nameof(PodeGerenciarCurso), nameof(TemOpcoesModulo), nameof(BotaoEditarTexto))]
        public partial bool Editando { get; private set; }

        /// <summary>Coordenação ou professor do curso: vê o botão "Editar".</summary>
        public bool PodeEditar => IsCoordenacao || IsProfessorDoCurso;
        public string BotaoEditarTexto => Editando ? "Concluir" : "Editar";

        /// <summary>Módulos, matérias e aulas: coordenação e professores do curso, no modo de edição.</summary>
        public bool PodeEditarEstrutura => Editando && PodeEditar;
        public bool PodeEditarAulas => Editando && PodeEditar;
        public bool PodeCriarProvas => Editando && IsProfessorDoCurso;

        /// <summary>Editar os dados do curso e excluir o curso inteiro: só a coordenação.</summary>
        public bool PodeGerenciarCurso => Editando && IsCoordenacao;

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

            OnPropertyChanged(nameof(PodeEditar));
            OnPropertyChanged(nameof(PodeEditarEstrutura));
            OnPropertyChanged(nameof(PodeEditarAulas));
            OnPropertyChanged(nameof(PodeCriarProvas));
            OnPropertyChanged(nameof(PodeGerenciarCurso));
            // continua no módulo que estava aberto (ex.: ao voltar da edição de uma aula); senão, o primeiro
            var escolhido = ModuloSelecionado?.Valor?.Id;
            Modulos = _modulos.Select(n => new ModuloOpcao { Valor = n.Modulo, Texto = n.Modulo.Titulo }).ToList();
            ModuloSelecionado = Modulos.FirstOrDefault(o => o.Valor!.Id == escolhido) ?? Modulos.FirstOrDefault();
            AoMudarModulo();
        });

        private void AoMudarModulo()
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
        private void AlternarEdicao() => Editando = !Editando;

        [RelayCommand]
        private Task NovoModulo()
            => Curso == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ModuloEdit, ("Curso", Curso));

        [RelayCommand]
        private Task AbrirProva(ProvaNode? node)
            => node?.PodeEditar == true ? Navegacao.IrAsync(Navegacao.ProvaResultados, ("Prova", node.Prova)) : Task.CompletedTask;

        [RelayCommand]
        private async Task OpcoesCursoAsync()
        {
            if (Curso == null || !PodeGerenciarCurso)
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
                    await ExcluirModuloAsync();
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
                    await ExcluirMateriaAsync(node);
                    break;
            }
        }

        /// <summary>Visualizando: abre a aula (vídeo e materiais). Editando: menu da aula.</summary>
        [RelayCommand]
        private async Task OpcoesAulaAsync(Aula? aula)
        {
            if (aula == null || Curso == null)
                return;

            if (!PodeEditarAulas)
            {
                await Navegacao.IrAsync(Navegacao.Aula, ("Aula", aula));
                return;
            }

            switch (await EscolherOpcaoAsync(aula.Titulo, "Excluir aula", "Editar aula", "Adicionar material"))
            {
                case "Editar aula":
                case "Adicionar material":
                    // os materiais ficam na tela da aula, abaixo dos dados
                    await Navegacao.IrAsync(Navegacao.AulaEdit, ("Materia", aula.Materia), ("Aula", aula));
                    break;
                case "Excluir aula":
                    await ExcluirAulaAsync(aula);
                    break;
            }
        }

        // ---- Excluir (lixeira em cada item, no modo de edição; também nos menus ⋮) ----

        [RelayCommand]
        private Task ExcluirModuloAsync()
        {
            var modulo = ModuloAtual?.Modulo;
            if (modulo == null || Curso == null || !PodeEditarEstrutura)
                return Task.CompletedTask;

            return ExcluirEAtualizarAsync($"Excluir o módulo \"{modulo.Titulo}\" com todas as matérias, aulas e provas?",
                () => CursoRepositorio.ExcluirModuloAsync(Curso.Id, modulo));
        }

        [RelayCommand]
        private Task ExcluirMateriaAsync(MateriaNode? node)
        {
            if (node == null || Curso == null || !PodeEditarEstrutura)
                return Task.CompletedTask;

            var materia = node.Materia;
            return ExcluirEAtualizarAsync($"Excluir a matéria \"{materia.Nome}\" com suas aulas e provas?",
                () => CursoRepositorio.ExcluirMateriaAsync(Curso.Id, materia));
        }

        [RelayCommand]
        private Task ExcluirAulaAsync(Aula? aula)
        {
            if (aula == null || Curso == null || !PodeEditarAulas)
                return Task.CompletedTask;

            return ExcluirEAtualizarAsync($"Excluir a aula \"{aula.Titulo}\" e seus materiais?",
                () => CursoRepositorio.ExcluirAulaAsync(Curso.Id, aula));
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
