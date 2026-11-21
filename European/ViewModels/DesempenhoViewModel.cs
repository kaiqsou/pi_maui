using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class MateriaDesempenho
    {
        public string Nome { get; set; } = string.Empty;
        public string ModuloTitulo { get; set; } = string.Empty;
        public double Media { get; set; }
        public double Progresso { get; set; }
        public bool Aprovado { get; set; }
        public bool Reprovado { get; set; }

        /// <summary>"Aprovado", "Abaixo da nota de corte" ou quantas provas da matéria já foram corrigidas.</summary>
        public string SituacaoTexto { get; set; } = string.Empty;
    }

    public class ResultadoItem
    {
        public ResultadoProva Resultado { get; set; } = new();
        public Prova? Prova { get; set; }

        /// <summary>Corrigida: tocar abre a revisão (erros, acertos e comentários do professor).</summary>
        public bool PodeRever => Corrigida && Prova != null;

        public string ProvaTitulo { get; set; } = string.Empty;
        public string MateriaNome { get; set; } = string.Empty;
        public double Nota { get; set; }
        public string AcertosTexto { get; set; } = string.Empty;
        public bool Aprovado { get; set; }
        public bool Corrigida { get; set; }
        public string Data => DataRealizacao.ToString("dd/MM/yyyy");
        public DateTime DataRealizacao { get; set; }

        /// <summary>Nota só aparece depois da correção do professor.</summary>
        public string NotaTexto => Corrigida ? Nota.ToString("0.0") : "—";

        public static ResultadoItem De(ResultadoProva resultado, Prova? prova) => new()
        {
            Resultado = resultado,
            Prova = prova,
            ProvaTitulo = prova?.Titulo ?? "Avaliação",
            MateriaNome = prova?.Materia?.Nome ?? string.Empty,
            Nota = resultado.NotaObtida,
            Corrigida = resultado.Corrigida,
            AcertosTexto = resultado.Corrigida ? "Corrigida" : "Aguardando correção do professor",
            Aprovado = resultado.Corrigida && resultado.NotaObtida >= (prova?.NotaCorte ?? ProgressoService.NotaCortePadrao),
            DataRealizacao = resultado.DataRealizacao
        };
    }

    /// <summary>Aba "Desempenho": média e situação do aluno em cada matéria.</summary>
    public partial class DesempenhoViewModel : BaseViewModel
    {
        public DesempenhoViewModel()
        {
            Title = "Desempenho";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemMaterias))]
        public partial ObservableCollection<MateriaDesempenho> Materias { get; private set; } = new();

        /// <summary>Materias mostrados aos poucos (6 por vez).</summary>
        public ListaIncremental<MateriaDesempenho> MateriasLista => _materiasLista ??= new(6, Materias);
        private ListaIncremental<MateriaDesempenho>? _materiasLista;

        partial void OnMateriasChanged(ObservableCollection<MateriaDesempenho> value) => MateriasLista.Fonte = value;

        public bool SemMaterias => Materias.Count == 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MediaTexto))]
        public partial double MediaGeral { get; private set; }

        public string MediaTexto => MediaGeral.ToString("0.0");

        [ObservableProperty]
        public partial bool Aprovado { get; private set; }

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            var materias = new ObservableCollection<MateriaDesempenho>();
            var notasGerais = new List<(ResultadoProva, Prova)>();

            if (aluno != null)
            {
                var matriculas = await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id);
                var agenda = await ProvaService.AgendaAsync(matriculas);
                var provas = agenda.Select(p => p.Prova).ToList();
                // entram na média as provas que o professor já corrigiu e as perdidas (prazo encerrado sem entrega = 0)
                var doAluno = ProvaService.ComPerdidas(agenda, (await ResultadoRepositorio.DoAlunoAsync(aluno.Id)).Where(r => r.Corrigida));

                foreach (var matricula in matriculas)
                {
                    var progressos = await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);

                    foreach (var modulo in matricula.Curso!.Modulos.OrderBy(m => m.Ordem))
                    {
                        foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                        {
                            foreach (var prova in provas.Where(p => p.MateriaId == materia.Id))
                                prova.Materia = materia;

                            var notas = ProgressoService.ComProva(doAluno.Where(r => r.MateriaId == materia.Id), provas);
                            notasGerais.AddRange(notas);

                            var total = materia.Aulas.Count;
                            var concluidas = materia.Aulas.Count(a => progressos.Any(p => p.AulaId == a.Id && p.Concluida));

                            // mesma regra do certificado: aprovação pela média da matéria
                            var provasDaMateria = provas.Where(p => p.MateriaId == materia.Id).ToList();
                            var avaliacao = ProgressoService.AvaliarMateria(provasDaMateria, doAluno);
                            var feitas = provasDaMateria.Count(p => doAluno.Any(r => r.ProvaId == p.Id));

                            materias.Add(new MateriaDesempenho
                            {
                                Nome = materia.Nome,
                                ModuloTitulo = modulo.Titulo,
                                Media = avaliacao.Media,
                                Progresso = total == 0 ? 0 : Math.Round(concluidas * 100.0 / total),
                                Aprovado = avaliacao.Aprovada,
                                Reprovado = avaliacao.Completa && provasDaMateria.Count > 0 && !avaliacao.Aprovada,
                                SituacaoTexto = provasDaMateria.Count == 0 ? "Sem provas ainda"
                                    : avaliacao.Aprovada ? $"Aprovado · nota de corte {avaliacao.NotaCorte:0.0}"
                                    : avaliacao.Completa ? $"Abaixo da nota de corte ({avaliacao.NotaCorte:0.0})"
                                    : $"{feitas} de {provasDaMateria.Count} prova(s) corrigida(s) · nota de corte {avaliacao.NotaCorte:0.0}"
                            });
                        }
                    }
                }
            }

            Materias = materias;

            MediaGeral = Math.Round(ProgressoService.Media(notasGerais), 1);
            // mesma regra das matérias: a maior nota de corte entre as provas que entraram na média
            Aprovado = notasGerais.Count > 0 && MediaGeral >= Math.Round(ProgressoService.NotaCorte(notasGerais.Select(n => n.Item2)), 1);
        });
    }
}
