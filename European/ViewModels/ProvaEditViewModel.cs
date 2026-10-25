using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class Opcao<T>
    {
        public T? Valor { get; init; }
        public string Texto { get; init; } = string.Empty;
    }

    public class MateriaOpcao : Opcao<Materia> { }
    public class SalaOpcao : Opcao<Sala> { }

    public class AlternativaEdicao : BaseViewModel
    {
        private string _texto = string.Empty;
        private bool _correta;
        private string _letra = "A";

        public QuestaoEdicao Questao { get; init; } = null!;
        public string Texto { get => _texto; set => SetProperty(ref _texto, value); }
        public bool Correta { get => _correta; set => SetProperty(ref _correta, value); }
        public string Letra { get => _letra; set => SetProperty(ref _letra, value); }
    }

    public class QuestaoEdicao : BaseViewModel
    {
        private int _numero;
        private string _enunciado = string.Empty;
        private string _peso = "1";

        public int Numero { get => _numero; set => SetProperty(ref _numero, value); }
        public string Enunciado { get => _enunciado; set => SetProperty(ref _enunciado, value); }
        public string Peso { get => _peso; set => SetProperty(ref _peso, value); }
        public ObservableCollection<AlternativaEdicao> Alternativas { get; } = new();

        public void Renumerar()
        {
            for (var i = 0; i < Alternativas.Count; i++)
                Alternativas[i].Letra = ((char)('A' + i)).ToString();
        }

        public AlternativaEdicao NovaAlternativa(string texto = "", bool correta = false)
        {
            var alternativa = new AlternativaEdicao { Questao = this, Texto = texto, Correta = correta };
            Alternativas.Add(alternativa);
            Renumerar();
            return alternativa;
        }
    }

    public class ProvaEditViewModel : BaseViewModel
    {
        private Prova? _prova;
        private List<Curso> _cursos = new();
        private List<Sala> _salas = new();
        private Curso? _cursoSelecionado;
        private MateriaOpcao? _materiaSelecionada;
        private SalaOpcao? _salaSelecionada;
        private List<MateriaOpcao> _materias = new();
        private List<SalaOpcao> _salaOpcoes = new();
        private string _titulo = string.Empty;
        private string _instrucoes = string.Empty;
        private string _duracaoMinutos = "60";
        private string _notaMaxima = "10";
        private string _notaCorte = "6";
        private string _peso = "1";
        private bool _temPrazo;
        private DateTime _prazoData = DateTime.Today.AddDays(7);
        private TimeSpan _prazoHora = new(23, 59, 0);
        private bool _isPublicada;
        private int _entregas;

        public ProvaEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
            ExcluirCommand = new Command(async () => await ExcluirAsync());
            ResultadosCommand = new Command(async () => await Shell.Current.GoToAsync("provaresultados", new Dictionary<string, object> { ["Prova"] = _prova! }));
            AdicionarQuestaoCommand = new Command(AdicionarQuestao);
            RemoverQuestaoCommand = new Command<QuestaoEdicao>(async q => await RemoverQuestaoAsync(q));
            AdicionarAlternativaCommand = new Command<QuestaoEdicao>(q => q?.NovaAlternativa());
            RemoverAlternativaCommand = new Command<AlternativaEdicao>(RemoverAlternativa);
            MarcarCorretaCommand = new Command<AlternativaEdicao>(MarcarCorreta);
        }

        public List<Curso> Cursos { get => _cursos; private set => SetProperty(ref _cursos, value); }
        public List<MateriaOpcao> Materias { get => _materias; private set => SetProperty(ref _materias, value); }
        public List<SalaOpcao> Salas { get => _salaOpcoes; private set => SetProperty(ref _salaOpcoes, value); }

        public Curso? CursoSelecionado
        {
            get => _cursoSelecionado;
            set
            {
                if (SetProperty(ref _cursoSelecionado, value))
                    AtualizarOpcoesDoCurso(null, null);
            }
        }

        public MateriaOpcao? MateriaSelecionada { get => _materiaSelecionada; set => SetProperty(ref _materiaSelecionada, value); }
        public SalaOpcao? SalaSelecionada { get => _salaSelecionada; set => SetProperty(ref _salaSelecionada, value); }

        public string Titulo { get => _titulo; set => SetProperty(ref _titulo, value); }
        public string Instrucoes { get => _instrucoes; set => SetProperty(ref _instrucoes, value); }
        public string DuracaoMinutos { get => _duracaoMinutos; set => SetProperty(ref _duracaoMinutos, value); }
        public string NotaMaxima { get => _notaMaxima; set => SetProperty(ref _notaMaxima, value); }
        public string NotaCorte { get => _notaCorte; set => SetProperty(ref _notaCorte, value); }
        public string Peso { get => _peso; set => SetProperty(ref _peso, value); }
        public bool TemPrazo { get => _temPrazo; set => SetProperty(ref _temPrazo, value); }
        public DateTime PrazoData { get => _prazoData; set => SetProperty(ref _prazoData, value); }
        public TimeSpan PrazoHora { get => _prazoHora; set => SetProperty(ref _prazoHora, value); }
        public bool IsPublicada { get => _isPublicada; set => SetProperty(ref _isPublicada, value); }

        public ObservableCollection<QuestaoEdicao> Questoes { get; } = new();

        public bool IsEdicao => _prova != null && !string.IsNullOrEmpty(_prova.Id);
        public bool TemEntregas => _entregas > 0;
        public string EntregasTexto => $"{_entregas} aluno(s) já fizeram esta prova. Alterar questões não recalcula as notas já lançadas.";
        public string ResumoQuestoes => $"{Questoes.Count} questão(ões) · peso total {Questoes.Sum(q => Numero(q.Peso, 1)):0.#}";

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand ResultadosCommand { get; }
        public ICommand AdicionarQuestaoCommand { get; }
        public ICommand RemoverQuestaoCommand { get; }
        public ICommand AdicionarAlternativaCommand { get; }
        public ICommand RemoverAlternativaCommand { get; }
        public ICommand MarcarCorretaCommand { get; }

        public async Task CarregarAsync(Materia? materia, Prova? prova)
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            _prova = prova;
            Title = prova == null ? "Nova prova" : "Editar prova";

            await ExecutarAsync(async () =>
            {
                _cursos = await CursoRepositorio.DoProfessorAsync(professor.Id);
                _salas = await SalaRepositorio.DoProfessorAsync(professor.Id);
                _entregas = prova == null ? 0 : (await ResultadoRepositorio.DaProvaAsync(prova.Id)).Count;

                var cursoId = prova?.CursoId ?? materia?.Modulo?.Curso?.Id;
                _cursoSelecionado = _cursos.FirstOrDefault(c => c.Id == cursoId) ?? _cursos.FirstOrDefault();
                Cursos = _cursos;
                OnPropertyChanged(nameof(CursoSelecionado));
                AtualizarOpcoesDoCurso(prova?.MateriaId ?? materia?.Id, prova?.SalaId);

                Titulo = prova?.Titulo ?? (materia == null ? string.Empty : $"Avaliação — {materia.Nome}");
                Instrucoes = prova?.Instrucoes ?? string.Empty;
                DuracaoMinutos = (prova?.DuracaoMinutos ?? 60).ToString();
                NotaMaxima = Texto(prova?.NotaMaxima ?? 10);
                NotaCorte = Texto(prova?.NotaCorte ?? 6);
                Peso = Texto(prova?.Peso ?? 1);
                TemPrazo = prova?.Prazo != null;
                PrazoData = prova?.Prazo?.Date ?? DateTime.Today.AddDays(7);
                PrazoHora = prova?.Prazo?.TimeOfDay ?? new TimeSpan(23, 59, 0);
                IsPublicada = prova?.IsPublicada ?? false;

                Questoes.Clear();
                foreach (var questao in prova?.Questoes ?? new List<Questao>())
                {
                    var edicao = new QuestaoEdicao { Enunciado = questao.Enunciado, Peso = Texto(questao.Peso) };
                    for (var i = 0; i < questao.Alternativas.Count; i++)
                        edicao.NovaAlternativa(questao.Alternativas[i], i == questao.IndiceCorreta);
                    Questoes.Add(edicao);
                }

                if (Questoes.Count == 0)
                    AdicionarQuestao();

                RenumerarQuestoes();
                OnPropertyChanged(nameof(IsEdicao));
                OnPropertyChanged(nameof(TemEntregas));
                OnPropertyChanged(nameof(EntregasTexto));
            });
        }

        private void AtualizarOpcoesDoCurso(string? materiaId, string? salaId)
        {
            var curso = _cursoSelecionado;
            Materias = curso == null
                ? new List<MateriaOpcao>()
                : curso.Modulos.OrderBy(m => m.Ordem)
                    .SelectMany(m => m.Materias.OrderBy(x => x.Ordem).Select(x => new MateriaOpcao { Valor = x, Texto = $"M{m.Ordem} · {x.Nome}" }))
                    .ToList();
            MateriaSelecionada = Materias.FirstOrDefault(m => m.Valor!.Id == materiaId) ?? Materias.FirstOrDefault();

            var salas = new List<SalaOpcao> { new() { Texto = "Todos os alunos do curso" } };
            salas.AddRange(_salas
                .Where(s => s.CursoId == null || s.CursoId == curso?.Id)
                .Select(s => new SalaOpcao { Valor = s, Texto = $"Sala: {s.Nome} ({s.AlunoIds.Count} alunos)" }));
            Salas = salas;
            SalaSelecionada = Salas.FirstOrDefault(s => s.Valor?.Id == salaId) ?? Salas[0];
        }

        private void AdicionarQuestao()
        {
            var questao = new QuestaoEdicao();
            questao.NovaAlternativa(correta: true);
            questao.NovaAlternativa();
            questao.NovaAlternativa();
            questao.NovaAlternativa();
            Questoes.Add(questao);
            RenumerarQuestoes();
        }

        private async Task RemoverQuestaoAsync(QuestaoEdicao? questao)
        {
            if (questao == null)
                return;

            if (!string.IsNullOrWhiteSpace(questao.Enunciado) && !await ConfirmarAsync($"Remover a questão {questao.Numero}?", "Remover"))
                return;

            Questoes.Remove(questao);
            RenumerarQuestoes();
        }

        private static void RemoverAlternativa(AlternativaEdicao? alternativa)
        {
            if (alternativa == null)
                return;

            var questao = alternativa.Questao;
            if (questao.Alternativas.Count <= 2)
                return;

            questao.Alternativas.Remove(alternativa);
            if (alternativa.Correta)
                questao.Alternativas[0].Correta = true;
            questao.Renumerar();
        }

        private static void MarcarCorreta(AlternativaEdicao? alternativa)
        {
            if (alternativa == null)
                return;

            foreach (var outra in alternativa.Questao.Alternativas)
                outra.Correta = ReferenceEquals(outra, alternativa);
        }

        private void RenumerarQuestoes()
        {
            for (var i = 0; i < Questoes.Count; i++)
                Questoes[i].Numero = i + 1;
            OnPropertyChanged(nameof(ResumoQuestoes));
        }

        private async Task SalvarAsync()
        {
            var professor = SessionService.UsuarioAtual;
            var erro = Validar();
            if (erro != null || professor == null)
            {
                await AvisarAsync("Prova", erro ?? "Sessão expirada.");
                return;
            }

            var prova = _prova ?? new Prova { ProfessorId = professor.Id };
            prova.CursoId = CursoSelecionado!.Id;
            prova.MateriaId = MateriaSelecionada!.Valor!.Id;
            prova.SalaId = SalaSelecionada?.Valor?.Id;
            prova.Titulo = Titulo.Trim();
            prova.Instrucoes = Instrucoes.Trim();
            prova.DuracaoMinutos = (int)Math.Max(0, Numero(DuracaoMinutos, 0));
            prova.NotaMaxima = Numero(NotaMaxima, 10);
            prova.NotaCorte = Numero(NotaCorte, 6);
            prova.Peso = Numero(Peso, 1);
            prova.Prazo = TemPrazo ? PrazoData.Date + PrazoHora : null;
            prova.IsPublicada = IsPublicada;
            prova.Questoes = Questoes.Select(q => new Questao
            {
                Enunciado = q.Enunciado.Trim(),
                Peso = Numero(q.Peso, 1),
                Alternativas = q.Alternativas.Select(a => a.Texto.Trim()).ToList(),
                IndiceCorreta = q.Alternativas.ToList().FindIndex(a => a.Correta)
            }).ToList();

            if (await ExecutarAsync(() => ProvaRepositorio.SalvarAsync(prova)))
            {
                await AvisarAsync("Prova", prova.IsPublicada
                    ? "Prova salva e publicada para os alunos."
                    : "Prova salva como rascunho. Ative \"Publicar\" quando quiser liberar aos alunos.");
                await VoltarAsync();
            }
        }

        private string? Validar()
        {
            if (CursoSelecionado == null)
                return "Você ainda não está atribuído a nenhum curso. Peça à coordenação.";
            if (MateriaSelecionada?.Valor == null)
                return "Selecione a matéria da prova.";
            if (string.IsNullOrWhiteSpace(Titulo))
                return "Informe o título da prova.";
            if (Numero(NotaMaxima, 0) <= 0)
                return "A nota máxima deve ser maior que zero.";
            if (Numero(NotaCorte, 0) > Numero(NotaMaxima, 10))
                return "A nota de corte não pode ser maior que a nota máxima.";
            if (Questoes.Count == 0)
                return "Adicione pelo menos uma questão.";

            foreach (var questao in Questoes)
            {
                if (string.IsNullOrWhiteSpace(questao.Enunciado))
                    return $"Escreva o enunciado da questão {questao.Numero}.";
                if (questao.Alternativas.Count < 2 || questao.Alternativas.Any(a => string.IsNullOrWhiteSpace(a.Texto)))
                    return $"Preencha todas as alternativas da questão {questao.Numero} (mínimo 2).";
                if (questao.Alternativas.Count(a => a.Correta) != 1)
                    return $"Marque a alternativa correta da questão {questao.Numero}.";
                if (Numero(questao.Peso, 1) <= 0)
                    return $"O peso da questão {questao.Numero} deve ser maior que zero.";
            }

            return null;
        }

        private async Task ExcluirAsync()
        {
            if (_prova == null || !await ConfirmarAsync($"Excluir a prova \"{_prova.Titulo}\"? As notas dos alunos nesta prova também serão removidas."))
                return;

            if (await ExecutarAsync(() => ProvaRepositorio.ExcluirAsync(_prova.Id)))
                await VoltarAsync();
        }

        private static double Numero(string? texto, double padrao)
            => double.TryParse((texto ?? string.Empty).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var valor) ? valor : padrao;

        private static string Texto(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
