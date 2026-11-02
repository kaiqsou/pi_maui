using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CursoResumo
    {
        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;
        public string Categoria => Curso.Categoria.ToString();
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
        public string Status => Prova.IsPublicada ? (Prova.Expirada ? "Encerrada" : "Publicada") : "Rascunho";

        /// <summary>Entregas aguardando a correção do professor.</summary>
        public int ACorrigir { get; init; }
        public string BotaoCorrecaoTexto => ACorrigir > 0 ? $"Corrigir ({ACorrigir})" : "Notas";
    }

    public class SalaResumo
    {
        public Sala Sala { get; init; } = new();
        public string Nome => Sala.Nome;
        public string Detalhe { get; init; } = string.Empty;
    }

    public class ProfessorViewModel : BaseViewModel
    {
        private string _aba = "cursos";
        private string _saudacao = string.Empty;

        public ProfessorViewModel()
        {
            SelecionarAbaCommand = new Command<string>(a => Aba = a);
            AbrirPerfilCommand = new Command(async () => await Shell.Current.GoToAsync("perfil"));
            AbrirCursoCommand = new Command<CursoResumo>(async c => await Ir("curso", "Curso", c?.Curso));
            NovaProvaCommand = new Command(async () => await Ir("provaedit", "Novo", true));
            AbrirProvaCommand = new Command<ProvaResumo>(async p => await Ir("provaedit", "Prova", p?.Prova));
            ResultadosProvaCommand = new Command<ProvaResumo>(async p => await Ir("provaresultados", "Prova", p?.Prova));
            NovaSalaCommand = new Command(async () => await Ir("salaedit", "Novo", true));
            AbrirSalaCommand = new Command<SalaResumo>(async s => await Ir("salaedit", "Sala", s?.Sala));
        }

        public string Aba
        {
            get => _aba;
            set
            {
                if (SetProperty(ref _aba, value))
                {
                    OnPropertyChanged(nameof(MostrarCursos));
                    OnPropertyChanged(nameof(MostrarProvas));
                    OnPropertyChanged(nameof(MostrarSalas));
                }
            }
        }

        public bool MostrarCursos => Aba == "cursos";
        public bool MostrarProvas => Aba == "provas";
        public bool MostrarSalas => Aba == "salas";

        public string Saudacao { get => _saudacao; private set => SetProperty(ref _saudacao, value); }

        public ObservableCollection<CursoResumo> Cursos { get; } = new();
        public ObservableCollection<ProvaResumo> Provas { get; } = new();
        public ObservableCollection<SalaResumo> Salas { get; } = new();

        public ICommand SelecionarAbaCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand AbrirCursoCommand { get; }
        public ICommand NovaProvaCommand { get; }
        public ICommand AbrirProvaCommand { get; }
        public ICommand ResultadosProvaCommand { get; }
        public ICommand NovaSalaCommand { get; }
        public ICommand AbrirSalaCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            Saudacao = $"Olá, {professor.Nome.Split(' ')[0]}";

            var cursos = await CursoRepositorio.DoProfessorAsync(professor.Id);
            var ativosPorCurso = await MatriculaRepositorio.AtivasPorCursoAsync();
            Cursos.Clear();
            foreach (var curso in cursos)
                Cursos.Add(new CursoResumo { Curso = curso, Detalhe = $"{ativosPorCurso.GetValueOrDefault(curso.Id)} aluno(s) ativos" });

            var provas = await ProvaRepositorio.DoProfessorAsync(professor.Id);
            var resultados = (await ResultadoRepositorio.DasProvasAsync(provas.Select(p => p.Id))).ToLookup(r => r.ProvaId);
            var todosCursos = (await CursoRepositorio.PorIdsAsync(provas.Select(p => p.CursoId))).ToDictionary(c => c.Id);
            var salas = await SalaRepositorio.DoProfessorAsync(professor.Id);

            Provas.Clear();
            foreach (var prova in provas)
            {
                var curso = todosCursos.GetValueOrDefault(prova.CursoId);
                var materia = curso?.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId);
                var entregas = resultados[prova.Id].ToList();
                var corrigidas = entregas.Where(r => r.Corrigida).ToList();
                var aCorrigir = entregas.Count - corrigidas.Count;
                var sala = salas.FirstOrDefault(s => s.Id == prova.SalaId);

                Provas.Add(new ProvaResumo
                {
                    Prova = prova,
                    Origem = $"{materia?.Nome ?? "Matéria removida"}{(sala == null ? string.Empty : $" · {sala.Nome}")}",
                    Detalhe = $"{prova.Questoes.Count} questões · {entregas.Count} entrega(s)" +
                              (aCorrigir > 0 ? $" · {aCorrigir} a corrigir" : string.Empty) +
                              (corrigidas.Count == 0 ? string.Empty : $" · média {corrigidas.Average(r => r.NotaObtida):0.0}"),
                    ACorrigir = aCorrigir
                });
            }

            Salas.Clear();
            foreach (var sala in salas)
            {
                var curso = cursos.FirstOrDefault(c => c.Id == sala.CursoId);
                Salas.Add(new SalaResumo
                {
                    Sala = sala,
                    Detalhe = $"{sala.AlunoIds.Count} aluno(s){(curso == null ? string.Empty : $" · {curso.Nome}")}"
                });
            }
        });

        private static async Task Ir(string rota, string chave, object? valor)
        {
            if (valor != null)
                await Shell.Current.GoToAsync(rota, new Dictionary<string, object> { [chave] = valor });
        }
    }
}
