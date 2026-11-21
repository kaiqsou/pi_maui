using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Assistente de criação de prova em 3 etapas: dados → regras → questões (uma por vez). Só salva no fim.</summary>
    public partial class ProvaEditViewModel : BaseViewModel
    {
        private Prova? _prova;
        private int _indiceQuestao;
        private int _entregas;

        // ---- Etapas ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarDados), nameof(MostrarRegras), nameof(MostrarQuestoes), nameof(Etapa),
            nameof(EtapaTexto), nameof(PodeVoltarEtapa), nameof(BotaoAvancarTexto), nameof(EtapaRegrasAtingida), nameof(EtapaQuestoesAtingida))]
        public partial string Secao { get; set; } = "dados";

        public bool MostrarDados => Secao == "dados";
        public bool MostrarRegras => Secao == "regras";
        public bool MostrarQuestoes => Secao == "questoes";
        public int Etapa => Secao switch { "regras" => 2, "questoes" => 3, _ => 1 };
        public string EtapaTexto => Etapa switch
        {
            1 => "Etapa 1 de 3 · Dados da prova",
            2 => "Etapa 2 de 3 · Regras e notas",
            _ => "Etapa 3 de 3 · Questões"
        };
        public bool EtapaRegrasAtingida => Etapa >= 2;
        public bool EtapaQuestoesAtingida => Etapa >= 3;
        public bool PodeVoltarEtapa => Etapa > 1;
        public string BotaoAvancarTexto => Etapa < 3 ? "Continuar" : "Salvar prova";

        // ---- Etapa 1: dados ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemCursos))]
        public partial List<Curso> Cursos { get; private set; } = new();

        public bool SemCursos => Cursos.Count == 0;

        [ObservableProperty]
        public partial Curso? CursoSelecionado { get; set; }

        [ObservableProperty]
        public partial List<MateriaOpcao> Materias { get; private set; } = new();

        [ObservableProperty]
        public partial MateriaOpcao? MateriaSelecionada { get; set; }

        [ObservableProperty]
        public partial string Titulo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Instrucoes { get; set; } = string.Empty;

        // ---- Etapa 2: regras ----

        [ObservableProperty]
        public partial string DuracaoMinutos { get; set; } = "60";

        [ObservableProperty]
        public partial string NotaMaxima { get; set; } = "10";

        [ObservableProperty]
        public partial string NotaCorte { get; set; } = "6";

        [ObservableProperty]
        public partial string Peso { get; set; } = "1";

        /// <summary>Dias que o aluno tem para fazer a prova depois que ela é liberada para ele.</summary>
        [ObservableProperty]
        public partial string PrazoDias { get; set; } = Prova.PrazoDiasPadrao.ToString();

        [ObservableProperty]
        public partial bool IsPublicada { get; set; }

        // ---- Etapa 3: questões ----

        public ObservableCollection<QuestaoEdicao> Questoes { get; } = new();

        public QuestaoEdicao? QuestaoAtual => _indiceQuestao >= 0 && _indiceQuestao < Questoes.Count ? Questoes[_indiceQuestao] : null;
        public string IndicadorQuestao => Questoes.Count == 0 ? "0 / 0" : $"{_indiceQuestao + 1} / {Questoes.Count}";
        public bool PodeQuestaoAnterior => _indiceQuestao > 0;
        public bool PodeQuestaoProxima => _indiceQuestao < Questoes.Count - 1;
        public string ResumoQuestoes => $"{Questoes.Count} questão(ões) · peso total {Questoes.Sum(q => q.PesoValor):0.#}";

        public bool IsEdicao => !string.IsNullOrEmpty(_prova?.Id);
        public bool TemEntregas => _entregas > 0;
        public string EntregasTexto => $"{_entregas} aluno(s) já fizeram esta prova. Alterar questões não recalcula as notas já lançadas.";

        partial void OnCursoSelecionadoChanged(Curso? value) => AtualizarOpcoesDoCurso(null);

        protected override async Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            var professor = SessionService.UsuarioAtual;
            if (professor == null)
                return;

            var materia = parametros.Obter<Materia>("Materia");
            var prova = parametros.Obter<Prova>("Prova");
            _prova = prova;
            Title = prova == null ? "Nova prova" : "Editar prova";

            await ExecutarAsync(async () =>
            {
                var cursos = await CursoRepositorio.DoProfessorAsync(professor.Id);
                _entregas = prova == null ? 0 : (await ResultadoRepositorio.DaProvaAsync(prova.Id)).Count;

                Cursos = cursos;
                var cursoId = prova?.CursoId ?? materia?.Modulo?.Curso?.Id;
                CursoSelecionado = cursos.FirstOrDefault(c => c.Id == cursoId) ?? cursos.FirstOrDefault();
                AtualizarOpcoesDoCurso(prova?.MateriaId ?? materia?.Id);

                Titulo = prova?.Titulo ?? (materia == null ? string.Empty : $"Avaliação — {materia.Nome}");
                Instrucoes = prova?.Instrucoes ?? string.Empty;
                DuracaoMinutos = (prova?.DuracaoMinutos ?? 60).ToString();
                NotaMaxima = Formatos.Numero(prova?.NotaMaxima ?? 10);
                NotaCorte = Formatos.Numero(prova?.NotaCorte ?? 6);
                Peso = Formatos.Numero(prova?.Peso ?? 1);
                PrazoDias = (prova?.PrazoDias ?? Prova.PrazoDiasPadrao).ToString();
                IsPublicada = prova?.IsPublicada ?? false;

                Questoes.Clear();
                foreach (var questao in prova?.Questoes ?? new List<Questao>())
                    Questoes.Add(QuestaoEdicao.De(questao));

                if (Questoes.Count == 0)
                    AdicionarQuestao();

                RenumerarQuestoes();
                Secao = "dados";
                IrParaQuestao(0);
                OnPropertyChanged(nameof(IsEdicao));
                OnPropertyChanged(nameof(TemEntregas));
                OnPropertyChanged(nameof(EntregasTexto));
            });
        }

        private void AtualizarOpcoesDoCurso(string? materiaId)
        {
            var curso = CursoSelecionado;
            Materias = curso == null
                ? new List<MateriaOpcao>()
                : curso.Modulos.OrderBy(m => m.Ordem)
                    .SelectMany(m => m.Materias.OrderBy(x => x.Ordem).Select(x => new MateriaOpcao { Valor = x, Texto = $"M{m.Ordem} · {x.Nome}" }))
                    .ToList();
            MateriaSelecionada = Materias.FirstOrDefault(m => m.Valor!.Id == materiaId) ?? Materias.FirstOrDefault();
        }

        // ---- Navegação entre etapas e questões ----

        /// <summary>"Continuar" valida só a etapa atual; na última etapa vira "Salvar prova".</summary>
        [RelayCommand]
        private async Task AvancarAsync()
        {
            var erro = Etapa switch
            {
                1 => ValidarDados(),
                2 => ValidarRegras(),
                _ => null
            };

            if (erro != null)
                await AvisarAsync("Prova", erro);
            else if (Etapa == 1)
                Secao = "regras";
            else if (Etapa == 2)
                Secao = "questoes";
            else
                await SalvarAsync();
        }

        [RelayCommand]
        private void VoltarEtapa() => Secao = Secao == "questoes" ? "regras" : "dados";

        [RelayCommand]
        private void QuestaoAnterior() => IrParaQuestao(_indiceQuestao - 1);

        [RelayCommand]
        private void QuestaoProxima() => IrParaQuestao(_indiceQuestao + 1);

        private void IrParaQuestao(int indice)
        {
            _indiceQuestao = Questoes.Count == 0 ? 0 : Math.Clamp(indice, 0, Questoes.Count - 1);
            OnPropertyChanged(nameof(QuestaoAtual));
            OnPropertyChanged(nameof(IndicadorQuestao));
            OnPropertyChanged(nameof(PodeQuestaoAnterior));
            OnPropertyChanged(nameof(PodeQuestaoProxima));
        }

        // ---- Edição das questões ----

        [RelayCommand]
        private void AdicionarQuestao()
        {
            Questoes.Add(QuestaoEdicao.Nova());
            RenumerarQuestoes();
            IrParaQuestao(Questoes.Count - 1); // já abre a questão nova
        }

        [RelayCommand]
        private async Task RemoverQuestaoAtualAsync()
        {
            var questao = QuestaoAtual;
            if (questao == null)
                return;

            if (!string.IsNullOrWhiteSpace(questao.Enunciado) && !await ConfirmarAsync($"Remover a questão {questao.Numero}?", "Remover"))
                return;

            Questoes.Remove(questao);
            RenumerarQuestoes();
            IrParaQuestao(_indiceQuestao);
        }

        [RelayCommand]
        private void TipoQuestao(string tipo)
        {
            if (QuestaoAtual != null)
                QuestaoAtual.Tipo = tipo == "dissertativa" ? Enums.TipoQuestao.Dissertativa : Enums.TipoQuestao.Alternativas;
        }

        [RelayCommand]
        private void AdicionarAlternativa(QuestaoEdicao? questao) => questao?.NovaAlternativa();

        [RelayCommand]
        private void RemoverAlternativa(AlternativaEdicao? alternativa) => alternativa?.Questao.RemoverAlternativa(alternativa);

        [RelayCommand]
        private void MarcarCorreta(AlternativaEdicao? alternativa) => alternativa?.Questao.MarcarCorreta(alternativa);

        private void RenumerarQuestoes()
        {
            for (var i = 0; i < Questoes.Count; i++)
                Questoes[i].Numero = i + 1;
            OnPropertyChanged(nameof(ResumoQuestoes));
        }

        // ---- Salvar, excluir e resultados ----

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

            // prova nova (ou que mudou de curso) entra no fim da ordem do curso; as provas antigas sem posição
            // ganham a posição em que já apareciam, para a nova não passar na frente delas
            if (string.IsNullOrEmpty(prova.Id) || prova.CursoId != CursoSelecionado!.Id)
            {
                var doCurso = ProvaService.OrdenarDoCurso(await ProvaRepositorio.DosCursosAsync(new[] { CursoSelecionado!.Id }), CursoSelecionado)
                    .Where(p => p.Id != prova.Id)
                    .ToList();
                if (doCurso.Any(p => p.Ordem == 0))
                    await ProvaRepositorio.SalvarOrdemAsync(doCurso);
                prova.Ordem = doCurso.Count + 1;
            }

            prova.CursoId = CursoSelecionado.Id;
            prova.MateriaId = MateriaSelecionada!.Valor!.Id;
            prova.Titulo = Titulo.Trim();
            prova.Instrucoes = Instrucoes.Trim();
            prova.DuracaoMinutos = (int)Math.Max(0, Formatos.LerNumero(DuracaoMinutos, 0));
            prova.NotaMaxima = Formatos.LerNumero(NotaMaxima, 10);
            prova.NotaCorte = Formatos.LerNumero(NotaCorte, 6);
            prova.Peso = Formatos.LerNumero(Peso, 1);
            prova.PrazoDias = (int)Formatos.LerNumero(PrazoDias, Prova.PrazoDiasPadrao);
            prova.IsPublicada = IsPublicada;
            prova.Questoes = Questoes.Select(q => q.ParaQuestao()).ToList();

            if (await ExecutarAsync(() => ProvaRepositorio.SalvarAsync(prova)))
            {
                await AvisarAsync("Prova", prova.IsPublicada
                    ? "Prova salva e publicada. Ela entra no calendário de cada aluno do curso, na ordem das matérias; as notas só aparecem para eles depois que você corrigir."
                    : "Prova salva como rascunho. Ative \"Publicar\" quando quiser liberar aos alunos.");
                await VoltarAsync();
            }
        }

        private string? ValidarDados()
        {
            if (CursoSelecionado == null)
                return "Você ainda não está atribuído a nenhum curso. Peça à coordenação.";
            if (MateriaSelecionada?.Valor == null)
                return "Selecione a matéria da prova.";
            if (string.IsNullOrWhiteSpace(Titulo))
                return "Informe o título da prova.";
            return null;
        }

        private string? ValidarRegras()
        {
            if (Formatos.LerNumero(NotaMaxima, 0) <= 0)
                return "A nota máxima deve ser maior que zero.";
            if (Formatos.LerNumero(NotaCorte, 0) > Formatos.LerNumero(NotaMaxima, 10))
                return "A nota de corte não pode ser maior que a nota máxima.";
            if (Formatos.LerNumero(PrazoDias, 0) < 1)
                return "O prazo para o aluno fazer a prova deve ser de pelo menos 1 dia.";
            return null;
        }

        /// <summary>Retorna o primeiro erro e já abre a etapa (e a questão) onde ele está.</summary>
        private string? Validar()
        {
            string Em(string secao, string erro)
            {
                Secao = secao;
                return erro;
            }

            if (ValidarDados() is { } erroDados)
                return Em("dados", erroDados);
            if (ValidarRegras() is { } erroRegras)
                return Em("regras", erroRegras);
            if (Questoes.Count == 0)
                return Em("questoes", "Adicione pelo menos uma questão.");

            for (var i = 0; i < Questoes.Count; i++)
            {
                if (Questoes[i].Validar() is { } erro)
                {
                    IrParaQuestao(i);
                    return Em("questoes", erro);
                }
            }

            return null;
        }

        [RelayCommand]
        private Task Excluir()
            => _prova == null
                ? Task.CompletedTask
                : ExcluirEVoltarAsync($"Excluir a prova \"{_prova.Titulo}\"? As notas dos alunos nesta prova também serão removidas.",
                    () => ProvaRepositorio.ExcluirAsync(_prova.Id));
    }
}
