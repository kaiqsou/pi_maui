using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CursoResumo
    {
        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;
        public string Estrutura => $"{Curso.Modulos.Count} módulos · {Curso.TotalMaterias} matérias · {Curso.TotalAulas} aulas";
        public string Detalhe { get; init; } = string.Empty;
        public bool Ativo => Curso.IsPublicado;
    }

    public class ProvaResumo
    {
        public Prova Prova { get; init; } = new();
        public string Titulo => Prova.Titulo;
        public string Origem { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
        public bool Publicada => Prova.IsPublicada;
        public string Status => Prova.IsPublicada ? "Publicada" : "Rascunho";

        /// <summary>Posição na ordem do curso (1 = primeira a ser liberada para o aluno).</summary>
        public int Posicao { get; init; }
        public string PosicaoTexto => $"{Posicao}º";
        public bool PodeSubir { get; init; }
        public bool PodeDescer { get; init; }

        /// <summary>Entregas aguardando a correção do professor.</summary>
        public int ACorrigir { get; init; }
        public bool TemACorrigir => ACorrigir > 0;
        public string ACorrigirTexto => $"{ACorrigir} a corrigir";
    }

    /// <summary>
    /// Painel do professor, com duas abas: cursos que leciona e provas. As provas são do curso (de todos os professores
    /// dele), aparecem separadas por curso e na ordem em que são liberadas para o aluno, que o professor muda pelas setas.
    /// </summary>
    public partial class ProfessorViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarCursos), nameof(MostrarProvas))]
        public partial string Aba { get; set; } = "cursos";

        public bool MostrarCursos => Aba == "cursos";
        public bool MostrarProvas => Aba == "provas";

        /// <summary>Notificações não lidas (sino no cabeçalho).</summary>
        public Sino Sino { get; } = new();

        [ObservableProperty]
        public partial string Saudacao { get; private set; } = string.Empty;

        public ObservableCollection<CursoResumo> Cursos { get; } = new();

        /// <summary>Cursos mostrados aos poucos (5 por vez).</summary>
        public ListaIncremental<CursoResumo> CursosLista => _cursosLista ??= new(5, Cursos);
        private ListaIncremental<CursoResumo>? _cursosLista;
        public ObservableCollection<ProvaResumo> Provas { get; } = new();

        /// <summary>Provas mostrados aos poucos (5 por vez).</summary>
        public ListaIncremental<ProvaResumo> ProvasLista => _provasLista ??= new(5, Provas);
        private ListaIncremental<ProvaResumo>? _provasLista;

        /// <summary>Provas de cada curso, já na ordem de liberação.</summary>
        private Dictionary<string, List<Prova>> _provasPorCurso = new();
        private ILookup<string, ResultadoProva> _entregas = Enumerable.Empty<ResultadoProva>().ToLookup(r => r.ProvaId);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemVariosCursos))]
        public partial List<CursoOpcao> CursosDasProvas { get; private set; } = new();

        /// <summary>Com mais de um curso, o professor escolhe de qual curso ver as provas.</summary>
        public bool TemVariosCursos => CursosDasProvas.Count > 1;

        [ObservableProperty]
        public partial CursoOpcao? CursoDasProvas { get; set; }

        partial void OnCursoDasProvasChanged(CursoOpcao? value) => ExibirProvas();

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            Saudacao = Formatos.Saudacao(professor.Nome);
            await Sino.AtualizarAsync();

            var cursos = await CursoRepositorio.DoProfessorAsync(professor.Id);
            var ativosPorCurso = await MatriculaRepositorio.AtivasPorCursoAsync();
            Cursos.Clear();
            foreach (var curso in cursos)
                Cursos.Add(new CursoResumo { Curso = curso, Detalhe = $"{ativosPorCurso.GetValueOrDefault(curso.Id)} aluno(s) ativos" });

            // provas dos cursos que ele leciona, de qualquer professor do curso (a entrega é corrigida por todos)
            var provas = await ProvaRepositorio.DosCursosAsync(cursos.Select(c => c.Id));
            _entregas = (await ResultadoRepositorio.DasProvasAsync(provas.Select(p => p.Id))).ToLookup(r => r.ProvaId);
            _provasPorCurso = cursos.ToDictionary(c => c.Id, c => ProvaService.OrdenarDoCurso(provas, c));

            var selecionado = CursoDasProvas?.Valor?.Id;
            CursosDasProvas = cursos.Select(c => new CursoOpcao { Valor = c, Texto = c.Nome }).ToList();
            CursoDasProvas = CursosDasProvas.FirstOrDefault(c => c.Valor!.Id == selecionado) ?? CursosDasProvas.FirstOrDefault();
            ExibirProvas();
        });

        private void ExibirProvas()
        {
            Provas.Clear();
            var curso = CursoDasProvas?.Valor;
            if (curso == null || !_provasPorCurso.TryGetValue(curso.Id, out var provas))
                return;

            for (var i = 0; i < provas.Count; i++)
            {
                var prova = provas[i];
                var entregas = _entregas[prova.Id].ToList();
                Provas.Add(new ProvaResumo
                {
                    Prova = prova,
                    Posicao = i + 1,
                    PodeSubir = i > 0,
                    PodeDescer = i < provas.Count - 1,
                    Origem = curso.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId)?.Nome ?? "Matéria removida",
                    Detalhe = $"{prova.Questoes.Count} questão(ões) · {prova.PrazoDiasTexto} · {entregas.Count} entrega(s)",
                    ACorrigir = entregas.Count(r => !r.Corrigida)
                });
            }
        }

        [RelayCommand]
        private Task Subir(ProvaResumo? item) => MoverAsync(item, -1);

        [RelayCommand]
        private Task Descer(ProvaResumo? item) => MoverAsync(item, +1);

        /// <summary>Troca a prova de lugar com a vizinha e grava a nova ordem do curso inteiro.</summary>
        private async Task MoverAsync(ProvaResumo? item, int direcao)
        {
            var curso = CursoDasProvas?.Valor;
            if (item == null || curso == null || !_provasPorCurso.TryGetValue(curso.Id, out var provas))
                return;

            var de = provas.FindIndex(p => p.Id == item.Prova.Id);
            var para = de + direcao;
            if (de < 0 || para < 0 || para >= provas.Count)
                return;

            var anterior = provas.ToList();
            (provas[de], provas[para]) = (provas[para], provas[de]);
            ExibirProvas();

            if (!await ExecutarAsync(() => ProvaRepositorio.SalvarOrdemAsync(provas)))
            {
                _provasPorCurso[curso.Id] = anterior;
                ExibirProvas();
                return;
            }

            for (var i = 0; i < provas.Count; i++)
                provas[i].Ordem = i + 1;
        }

        [RelayCommand]
        private void SelecionarAba(string aba) => Aba = aba;

        [RelayCommand]
        private Task AbrirPerfil() => Navegacao.IrAsync(Navegacao.Perfil);

        [RelayCommand]
        private Task AbrirCurso(CursoResumo? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.Curso, ("Curso", item.Curso));

        [RelayCommand]
        private Task NovaProva() => Navegacao.IrAsync(Navegacao.ProvaEdit);

        [RelayCommand]
        private Task AbrirProva(ProvaResumo? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ProvaResultados, ("Prova", item.Prova));
    }
}
