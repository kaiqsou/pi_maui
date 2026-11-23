using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class AulaItem
    {
        public Aula Aula { get; set; } = new();
        public bool Concluida { get; set; }
        public bool Liberada { get; set; } = true;

        /// <summary>Aula aberta agora (destaque na lista da tela da aula).</summary>
        public bool IsSelecionada { get; set; }
        public double Percentual { get; set; }
        public string Cabecalho { get; set; } = string.Empty;

        /// <summary>Primeira aula da matéria: mostra o nome da matéria acima dela.</summary>
        public bool MostrarCabecalho { get; set; }

        /// <summary>Vazia enquanto a duração não é conhecida (ela vem do vídeo na primeira vez que a aula é aberta).</summary>
        public string Duracao => Aula.DuracaoSegundos > 0 ? Aula.DuracaoTexto : string.Empty;
        public string Descricao => Aula.Descricao;
        public bool TemDescricao => !string.IsNullOrWhiteSpace(Aula.Descricao);
        public bool Bloqueada => !Liberada;
        public bool PodeAssistir => Liberada && !Concluida;
        public string MateriaNome => Aula.Materia?.Nome ?? string.Empty;
    }

    /// <summary>Aba "Aulas" do aluno: as aulas de um módulo por vez, escolhido no seletor de módulo.</summary>
    public partial class AulasViewModel : BaseViewModel
    {
        private List<Modulo> _modulos = new();
        private List<List<AulaItem>> _aulasPorModulo = new();

        public AulasViewModel()
        {
            Title = "Aulas";
        }

        /// <summary>Módulos do curso para o seletor (só aparece com mais de um módulo).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemVariosModulos), nameof(TemUmModulo))]
        public partial List<ModuloOpcao> Modulos { get; private set; } = new();

        public bool TemVariosModulos => Modulos.Count > 1;
        public bool TemUmModulo => Modulos.Count == 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ModuloAtualTitulo))]
        public partial ModuloOpcao? ModuloSelecionado { get; set; }

        partial void OnModuloSelecionadoChanged(ModuloOpcao? value) => AplicarModulo();

        [ObservableProperty]
        public partial string CursoNome { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial Matricula? MatriculaAtiva { get; private set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemAulas))]
        public partial IReadOnlyList<AulaItem> AulasPagina { get; private set; } = new List<AulaItem>();

        public string ModuloAtualTitulo => ModuloSelecionado?.Valor?.Titulo ?? string.Empty;
        public bool SemAulas => AulasPagina.Count == 0;
        public string MensagemSemAulas => _modulos.Count == 0
            ? "Ainda não há aulas disponíveis para você."
            : "Ainda não há aulas neste módulo.";

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            var modulos = new List<Modulo>();
            var aulasPorModulo = new List<List<AulaItem>>();

            // inclui a matrícula concluída: ao terminar a última aula o curso continua visível
            var matricula = aluno == null ? null : (await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id)).FirstOrDefault();
            MatriculaAtiva = matricula;
            CursoNome = matricula?.Curso?.Nome ?? string.Empty;

            if (matricula?.Curso != null)
            {
                var progressos = await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);
                foreach (var modulo in matricula.Curso.Modulos.OrderBy(m => m.Ordem))
                {
                    var aulas = new List<AulaItem>();
                    foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                    {
                        var primeiraDaMateria = true;
                        foreach (var aula in materia.Aulas.OrderBy(a => a.Ordem))
                        {
                            var progresso = progressos.FirstOrDefault(p => p.AulaId == aula.Id);
                            aulas.Add(new AulaItem
                            {
                                Aula = aula,
                                Concluida = progresso?.Concluida ?? false,
                                Liberada = ProgressoService.AulaEstaLiberada(matricula.Curso, progressos, aula),
                                Percentual = progresso?.PercentualAssistido ?? 0,
                                Cabecalho = materia.Nome,
                                MostrarCabecalho = primeiraDaMateria
                            });
                            primeiraDaMateria = false;
                        }
                    }

                    modulos.Add(modulo);
                    aulasPorModulo.Add(aulas);
                }
            }

            _modulos = modulos;
            _aulasPorModulo = aulasPorModulo;

            // mantém o módulo escolhido ao voltar para a aba; na primeira vez, abre no módulo em que o aluno está
            // (o da primeira aula liberada e ainda não concluída)
            var escolhido = ModuloSelecionado?.Valor?.Id;
            var atual = aulasPorModulo.FindIndex(a => a.Any(i => i.Liberada && !i.Concluida));
            Modulos = modulos.Select(m => new ModuloOpcao { Valor = m, Texto = m.Titulo }).ToList();
            ModuloSelecionado = Modulos.FirstOrDefault(o => o.Valor!.Id == escolhido)
                                ?? (atual >= 0 ? Modulos[atual] : Modulos.FirstOrDefault());
            AplicarModulo();
        });

        private void AplicarModulo()
        {
            var indice = ModuloSelecionado == null ? -1 : _modulos.FindIndex(m => m.Id == ModuloSelecionado.Valor?.Id);
            AulasPagina = indice >= 0 && indice < _aulasPorModulo.Count ? _aulasPorModulo[indice] : new List<AulaItem>();
            OnPropertyChanged(nameof(MensagemSemAulas));
        }

        [RelayCommand]
        private async Task AbrirAulaAsync(AulaItem? item)
        {
            if (item == null || MatriculaAtiva == null)
                return;

            if (!item.Liberada)
            {
                await AvisarAsync("Aula bloqueada", "Conclua a aula anterior para liberar esta aula.");
                return;
            }

            await Navegacao.IrAsync(Navegacao.Aula, ("Aula", item.Aula), ("Matricula", MatriculaAtiva));
        }
    }
}
