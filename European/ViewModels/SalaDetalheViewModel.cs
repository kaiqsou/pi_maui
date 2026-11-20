using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Visualização de uma turma do professor (dados, alunos e provas). A edição fica em SalaEdit.</summary>
    public partial class SalaDetalheViewModel : BaseViewModel
    {
        private string? _salaId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemDescricao), nameof(ResumoTexto))]
        public partial Sala? Sala { get; private set; }

        [ObservableProperty]
        public partial string CursoNome { get; private set; } = string.Empty;

        public bool TemDescricao => !string.IsNullOrWhiteSpace(Sala?.Descricao);
        public string ResumoTexto => $"{Alunos.Count} aluno(s) · {Provas.Count} prova(s)";

        public ObservableCollection<PessoaItem> Alunos { get; } = new();
        public ObservableCollection<ProvaResumo> Provas { get; } = new();

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _salaId = parametros.Obter<Sala>("Sala")?.Id ?? _salaId;
            return Task.CompletedTask;
        }

        /// <summary>Recarrega ao voltar da edição.</summary>
        public override Task AoAparecerAsync() => _salaId == null ? Task.CompletedTask : ExecutarAsync(async () =>
        {
            var sala = await SalaRepositorio.PorIdAsync(_salaId);
            if (sala == null)
            {
                await VoltarAsync(); // a turma foi excluída na edição
                return;
            }

            Title = sala.Nome;
            CursoNome = sala.CursoId == null
                ? "Todos os cursos do professor"
                : (await CursoRepositorio.PorIdAsync(sala.CursoId))?.Nome ?? "Curso removido";

            Alunos.Clear();
            foreach (var aluno in (await UsuarioRepositorio.PorIdsAsync(sala.AlunoIds)).OrderBy(a => a.Nome))
                Alunos.Add(new PessoaItem { Usuario = aluno, Detalhe = aluno.Email });

            Provas.Clear();
            foreach (var prova in (await ProvaRepositorio.DoProfessorAsync(sala.ProfessorId)).Where(p => p.SalaId == sala.Id))
                Provas.Add(new ProvaResumo { Prova = prova, Detalhe = $"{prova.Questoes.Count} questões · prazo: {prova.PrazoTexto}" });

            Sala = sala;
            OnPropertyChanged(nameof(ResumoTexto));
        });

        [RelayCommand]
        private Task Editar() => Sala == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.SalaEdit, ("Sala", Sala));

        [RelayCommand]
        private Task AbrirProva(ProvaResumo? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.ProvaResultados, ("Prova", item.Prova));
    }
}
