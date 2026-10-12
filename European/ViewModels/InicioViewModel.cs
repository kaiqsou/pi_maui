using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class InicioViewModel : BaseViewModel
    {
        private string _saudacao = string.Empty;
        private double _progressoMedio;
        private int _totalCursos;
        private int _totalCertificados;
        private Aula? _ultimaAula;
        private string _ultimaCursoNome = string.Empty;
        private Matricula? _matriculaAtiva;
        private ObservableCollection<Certificado> _certificadosRecentes = new();

        public InicioViewModel()
        {
            Title = "Início";
            ContinuarCommand = new Command(async () => await ContinuarAsync());
            IniciarAulasCommand = new Command(() => MainViewModel.Atual?.SelecionarAba("aulas"));
            AbrirPerfilCommand = new Command(async () => await Shell.Current.GoToAsync("perfil"));
        }

        public ObservableCollection<Certificado> CertificadosRecentes
        {
            get => _certificadosRecentes;
            private set => SetProperty(ref _certificadosRecentes, value);
        }

        public string Saudacao
        {
            get => _saudacao;
            set => SetProperty(ref _saudacao, value);
        }

        public double ProgressoMedio
        {
            get => _progressoMedio;
            set => SetProperty(ref _progressoMedio, value);
        }

        public int TotalCursos
        {
            get => _totalCursos;
            set => SetProperty(ref _totalCursos, value);
        }

        public int TotalCertificados
        {
            get => _totalCertificados;
            set => SetProperty(ref _totalCertificados, value);
        }

        public Aula? UltimaAula
        {
            get => _ultimaAula;
            set
            {
                if (SetProperty(ref _ultimaAula, value))
                {
                    OnPropertyChanged(nameof(TemUltimaAula));
                    OnPropertyChanged(nameof(MostrarIniciarAulas));
                    OnPropertyChanged(nameof(UltimaDuracao));
                }
            }
        }

        public bool TemUltimaAula => UltimaAula != null;

        public bool MostrarIniciarAulas => UltimaAula == null;

        public string UltimaDuracao =>
            UltimaAula == null ? string.Empty
                : TimeSpan.FromSeconds(MockDataService.DuracaoVideosDaAula(UltimaAula)).ToString(@"mm\:ss");

        public string UltimaCursoNome
        {
            get => _ultimaCursoNome;
            set => SetProperty(ref _ultimaCursoNome, value);
        }

        public Matricula? MatriculaAtiva
        {
            get => _matriculaAtiva;
            set => SetProperty(ref _matriculaAtiva, value);
        }

        public ICommand ContinuarCommand { get; }
        public ICommand IniciarAulasCommand { get; }
        public ICommand AbrirPerfilCommand { get; }

        public void Carregar()
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null)
                return;

            Saudacao = $"Olá, {PrimeiroNome(aluno.Nome)}";

            var matriculas = MockDataService.Matriculas().Where(m => m.AlunoId == aluno.Id).ToList();
            TotalCursos = matriculas.Count;
            ProgressoMedio = matriculas.Count == 0 ? 0 : Math.Round(matriculas.Average(m => m.ProgressoPercentual));

            var certificados = MockDataService.CertificadosDoAluno(aluno.Id);
            TotalCertificados = certificados.Count;
            var certificadosRecentes = new ObservableCollection<Certificado>();
            foreach (var certificado in certificados.Take(3))
                certificadosRecentes.Add(certificado);
            CertificadosRecentes = certificadosRecentes;

            MatriculaAtiva = matriculas
                .OrderByDescending(m => m.Status == Enums.StatusMatricula.Ativa)
                .ThenByDescending(m => m.ProgressoPercentual)
                .FirstOrDefault();

            UltimaAula = null;
            if (MatriculaAtiva?.Curso != null)
            {
                UltimaCursoNome = MatriculaAtiva.Curso.Nome;

                var ultimoProgresso = MockDataService.ProgressosDaMatricula(MatriculaAtiva.Id)
                    .OrderByDescending(p => p.AtualizadoEm)
                    .FirstOrDefault();

                if (ultimoProgresso != null)
                {
                    UltimaAula = MatriculaAtiva.Curso.Modulos
                        .SelectMany(m => m.Materias)
                        .SelectMany(x => x.Aulas)
                        .FirstOrDefault(a => a.Id == ultimoProgresso.AulaId);
                }
            }
        }

        private async Task ContinuarAsync()
        {
            if (UltimaAula == null || MatriculaAtiva == null)
                return;

            var parametros = new Dictionary<string, object>
            {
                ["Aula"] = UltimaAula,
                ["Matricula"] = MatriculaAtiva
            };
            await Shell.Current.GoToAsync("aula", parametros);
        }

        private static string PrimeiroNome(string nome)
            => string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Split(' ')[0];
    }
}
