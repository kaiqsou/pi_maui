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
        public string Status => Prova.IsPublicada ? (Prova.Expirada ? "Encerrada" : "Publicada") : "Rascunho";

        /// <summary>Entregas aguardando a correção do professor.</summary>
        public int ACorrigir { get; init; }
        public bool TemACorrigir => ACorrigir > 0;
        public string ACorrigirTexto => $"{ACorrigir} a corrigir";
    }

    public class SalaResumo
    {
        public Sala Sala { get; init; } = new();
        public string Nome => Sala.Nome;
        public string Detalhe { get; init; } = string.Empty;
    }

    /// <summary>Painel do professor, com três abas: cursos que leciona, provas e salas (turmas).</summary>
    public partial class ProfessorViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarCursos), nameof(MostrarProvas), nameof(MostrarSalas))]
        public partial string Aba { get; set; } = "cursos";

        public bool MostrarCursos => Aba == "cursos";
        public bool MostrarProvas => Aba == "provas";
        public bool MostrarSalas => Aba == "salas";

        /// <summary>Notificações não lidas (sino no cabeçalho).</summary>
        public Sino Sino { get; } = new();

        [ObservableProperty]
        public partial string Saudacao { get; private set; } = string.Empty;

        public ObservableCollection<CursoResumo> Cursos { get; } = new();
        public ObservableCollection<ProvaResumo> Provas { get; } = new();
        public ObservableCollection<SalaResumo> Salas { get; } = new();

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

            var provas = await ProvaRepositorio.DoProfessorAsync(professor.Id);
            var resultados = (await ResultadoRepositorio.DasProvasAsync(provas.Select(p => p.Id))).ToLookup(r => r.ProvaId);
            var cursosDasProvas = (await CursoRepositorio.PorIdsAsync(provas.Select(p => p.CursoId))).ToDictionary(c => c.Id);
            var salas = await SalaRepositorio.DoProfessorAsync(professor.Id);

            Provas.Clear();
            foreach (var prova in provas)
            {
                var materia = cursosDasProvas.GetValueOrDefault(prova.CursoId)?.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId);
                var entregas = resultados[prova.Id].ToList();
                var corrigidas = entregas.Where(r => r.Corrigida).ToList();
                var aCorrigir = entregas.Count - corrigidas.Count;
                var sala = salas.FirstOrDefault(s => s.Id == prova.SalaId);

                Provas.Add(new ProvaResumo
                {
                    Prova = prova,
                    Origem = $"{materia?.Nome ?? "Matéria removida"}{(sala == null ? string.Empty : $" · {sala.Nome}")}",
                    Detalhe = $"{prova.Questoes.Count} questão(ões) · {entregas.Count} entrega(s)",
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

        [RelayCommand]
        private Task NovaSala() => Navegacao.IrAsync(Navegacao.SalaEdit);

        [RelayCommand]
        private Task AbrirSala(SalaResumo? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.Sala, ("Sala", item.Sala));
    }
}
