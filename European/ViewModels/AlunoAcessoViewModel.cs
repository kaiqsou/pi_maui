using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class MatriculaItem
    {
        public Matricula Matricula { get; init; } = new();
        public string CursoNome { get; init; } = string.Empty;
        public string Status => Matricula.Status switch
        {
            StatusMatricula.Ativa => "Ativa",
            StatusMatricula.Concluida => "Concluída",
            StatusMatricula.Cancelada => "Encerrada",
            StatusMatricula.Trancada => "Trancada",
            _ => "Pendente"
        };
        public string Detalhe => $"{Matricula.ProgressoPercentual:0}% concluído · desde {Matricula.DataMatricula:dd/MM/yyyy}";
        public bool Ativa => Matricula.Status == StatusMatricula.Ativa;
        public bool Encerrada => Matricula.Status == StatusMatricula.Cancelada;
    }

    /// <summary>Coordenação: liberar/remover o acesso de um aluno e gerenciar as matrículas dele.</summary>
    public partial class AlunoAcessoViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Email), nameof(Iniciais), nameof(PodeRemover), nameof(Pago), nameof(PagamentoTexto))]
        public partial Usuario? Aluno { get; private set; }

        public string Nome => Aluno?.Nome ?? string.Empty;
        public string Email => Aluno?.Email ?? string.Empty;
        public string Iniciais => Aluno?.Iniciais ?? "?";
        public bool PodeRemover => Aluno?.StatusAcesso == StatusAcesso.Liberado;

        /// <summary>Pagamento feito na solicitação de matrícula (contas criadas pela coordenação não têm).</summary>
        public bool Pago => Aluno?.PagamentoEm != null;
        public string PagamentoTexto => Pago ? $"Pago {Formatos.Pagamento(Aluno!.FormaPagamento, Aluno.PagamentoEm)}" : string.Empty;

        [ObservableProperty]
        public partial string StatusTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string StatusDescricao { get; private set; } = string.Empty;

        public ObservableCollection<MatriculaItem> Matriculas { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemCursosDisponiveis))]
        public partial List<CursoOpcao> CursosDisponiveis { get; private set; } = new();

        [ObservableProperty]
        public partial CursoOpcao? CursoSelecionado { get; set; }

        public bool TemCursosDisponiveis => CursosDisponiveis.Count > 0;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
            => parametros.Obter<Usuario>("Usuario") is { } aluno ? CarregarAsync(aluno.Id) : Task.CompletedTask;

        private Task CarregarAsync(string alunoId) => ExecutarAsync(async () =>
        {
            Aluno = await UsuarioRepositorio.PorIdAsync(alunoId);
            if (Aluno == null)
                return;

            Title = Aluno.Nome;
            var matriculas = await MatriculaRepositorio.DoAlunoAsync(Aluno.Id);
            var cursos = await CursoRepositorio.ListarAsync();

            Matriculas.Clear();
            foreach (var matricula in matriculas)
                Matriculas.Add(new MatriculaItem { Matricula = matricula, CursoNome = matricula.Curso?.Nome ?? string.Empty });

            var ocupados = matriculas.Where(m => m.Status is StatusMatricula.Ativa or StatusMatricula.Concluida).Select(m => m.CursoId).ToHashSet();
            CursosDisponiveis = cursos.Where(c => c.IsPublicado && !ocupados.Contains(c.Id))
                .Select(c => new CursoOpcao { Valor = c, Texto = c.Nome })
                .ToList();
            CursoSelecionado = CursosDisponiveis.FirstOrDefault();
            OnPropertyChanged(nameof(PodeRemover));

            var ativas = matriculas.Count(m => m.Status == StatusMatricula.Ativa);
            (StatusTexto, StatusDescricao) = Aluno.StatusAcesso switch
            {
                StatusAcesso.Pendente => ("Aguardando liberação", "O aluno criou a conta e ainda não tem acesso. Escolha o curso abaixo e libere."),
                StatusAcesso.Removido => (matriculas.Count > 0 ? "Ex-aluno" : "Sem acesso", "O acesso foi removido. Se já foi aluno, ele vê apenas o próprio certificado."),
                _ when ativas > 0 => ("Acesso liberado", $"Acessa aulas e provas de {ativas} curso(s)."),
                _ => ("Sem curso ativo", "Conta liberada, mas sem matrícula ativa: o aluno vê apenas o certificado.")
            };
        });

        [RelayCommand]
        private async Task LiberarAsync()
        {
            var curso = CursoSelecionado?.Valor;
            if (Aluno == null || curso == null)
                return;

            if (await ExecutarAsync(() => ContaService.LiberarCursoAsync(Aluno, curso.Id)))
            {
                await AvisarAsync("Acesso liberado", $"{Aluno.Nome} já pode acessar {curso.Nome}.");
                await CarregarAsync(Aluno.Id);
            }
        }

        [RelayCommand]
        private async Task EncerrarAsync(MatriculaItem? item)
        {
            if (item == null || Aluno == null
                || !await ConfirmarAsync($"Encerrar o acesso de {Aluno.Nome} a {item.CursoNome}? O progresso fica guardado.", "Encerrar"))
                return;

            await ExecutarAsync(() => ContaService.AlterarMatriculaAsync(Aluno, item.Matricula, StatusMatricula.Cancelada));
            await CarregarAsync(Aluno.Id);
        }

        [RelayCommand]
        private async Task ReativarAsync(MatriculaItem? item)
        {
            if (item == null || Aluno == null)
                return;

            await ExecutarAsync(() => ContaService.AlterarMatriculaAsync(Aluno, item.Matricula, StatusMatricula.Ativa));
            await CarregarAsync(Aluno.Id);
        }

        [RelayCommand]
        private async Task RemoverAcessoAsync()
        {
            if (Aluno == null || !await ConfirmarAsync($"Remover o acesso de {Aluno.Nome} ao aplicativo? Todas as matrículas ativas serão encerradas.", "Remover acesso"))
                return;

            await ExecutarAsync(() => ContaService.RemoverAcessoAlunoAsync(Aluno, Matriculas.Select(m => m.Matricula)));
            await CarregarAsync(Aluno.Id);
        }

        [RelayCommand]
        private Task RedefinirSenha() => Aluno == null ? Task.CompletedTask : RedefinirSenhaProvisoriaAsync(Aluno, "aluno");
    }
}
