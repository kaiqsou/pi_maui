using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>O certificado único do mestrado: emitido ao concluir todos os módulos com aprovação pela média em todas as matérias.</summary>
    public class CertificadoViewModel : BaseViewModel
    {
        private Certificado? _certificado;
        private ProgressoService.SituacaoCertificado? _situacao;
        private string _cursoNome = string.Empty;
        private string _validadeTexto = string.Empty;

        public CertificadoViewModel()
        {
            Title = "Certificados";
            AbrirCommand = new Command(async () => await AbrirAsync());
            BaixarPdfCommand = new Command(async () => await BaixarPdfAsync());
        }

        public Certificado? Certificado
        {
            get => _certificado;
            private set
            {
                if (SetProperty(ref _certificado, value))
                {
                    OnPropertyChanged(nameof(PossuiCertificado));
                    OnPropertyChanged(nameof(Pendente));
                    OnPropertyChanged(nameof(EmissaoTexto));
                    OnPropertyChanged(nameof(MediaTexto));
                }
            }
        }

        public bool PossuiCertificado => Certificado != null;
        public bool Pendente => Certificado == null;
        public bool PossuiSituacao => _situacao != null;

        public string CursoNome
        {
            get => _cursoNome;
            private set => SetProperty(ref _cursoNome, value);
        }

        /// <summary>Até quando o aluno pode reassistir as aulas depois de concluir o curso.</summary>
        public string ValidadeTexto
        {
            get => _validadeTexto;
            private set
            {
                if (SetProperty(ref _validadeTexto, value))
                    OnPropertyChanged(nameof(PossuiValidade));
            }
        }

        public bool PossuiValidade => !string.IsNullOrEmpty(ValidadeTexto);

        public string EmissaoTexto => Certificado?.EmitidoEm.ToString("dd/MM/yyyy") ?? string.Empty;
        public string MediaTexto => Certificado?.MediaFinal.ToString("0.0") ?? string.Empty;

        public string ModulosTexto => _situacao == null ? string.Empty : $"{_situacao.ModulosConcluidos} de {_situacao.TotalModulos}";
        public string MateriasTexto => _situacao == null ? string.Empty : $"{_situacao.MateriasAprovadas} de {_situacao.TotalMateriasComProva}";
        public double ModulosPercentual => Percentual(_situacao?.ModulosConcluidos, _situacao?.TotalModulos);
        public double MateriasPercentual => _situacao?.TotalMateriasComProva == 0 ? 100 : Percentual(_situacao?.MateriasAprovadas, _situacao?.TotalMateriasComProva);
        public bool ModulosOk => _situacao != null && _situacao.TotalModulos > 0 && _situacao.ModulosConcluidos == _situacao.TotalModulos;
        public bool MateriasOk => _situacao != null && _situacao.MateriasAprovadas == _situacao.TotalMateriasComProva;

        public ICommand AbrirCommand { get; }
        public ICommand BaixarPdfCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            Certificado? certificado = null;
            ProgressoService.SituacaoCertificado? situacao = null;
            var cursoNome = string.Empty;
            var validade = string.Empty;
            Matricula? matricula = null;

            if (aluno != null)
            {
                matricula = (await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id)).FirstOrDefault();
                if (matricula?.Curso != null)
                {
                    cursoNome = matricula.Curso.Nome;
                    situacao = await ProgressoService.SituacaoAsync(matricula, matricula.Curso);

                    // emite agora se o aluno já cumpriu os requisitos (ex.: aprovado em uma prova depois da última aula)
                    if (situacao.Apto && !await CertificadoRepositorio.ExisteParaMatriculaAsync(matricula.Id))
                        await ProgressoService.AtualizarAsync(matricula);
                }

                certificado = (await CertificadoRepositorio.DoAlunoAsync(aluno.Id)).FirstOrDefault();
                if (certificado != null && string.IsNullOrEmpty(cursoNome))
                    cursoNome = certificado.CursoNome;

                if (certificado != null)
                {
                    // mesma data que libera o login (Matricula.AcessoAte); sem matrícula vigente, conta pela emissão
                    var acessoAte = matricula?.AcessoAte ?? certificado.EmitidoEm.AddMonths(Matricula.MesesAcessoAposConclusao);
                    validade = acessoAte > DateTime.Now
                        ? $"Você pode reassistir as aulas até {acessoAte:dd/MM/yyyy}."
                        : $"O acesso às aulas terminou em {acessoAte:dd/MM/yyyy}.";
                }
            }

            _situacao = situacao;
            CursoNome = cursoNome;
            ValidadeTexto = validade;
            Certificado = certificado;

            OnPropertyChanged(nameof(PossuiSituacao));
            OnPropertyChanged(nameof(ModulosTexto));
            OnPropertyChanged(nameof(MateriasTexto));
            OnPropertyChanged(nameof(ModulosPercentual));
            OnPropertyChanged(nameof(MateriasPercentual));
            OnPropertyChanged(nameof(ModulosOk));
            OnPropertyChanged(nameof(MateriasOk));
        });

        private static double Percentual(int? feitos, int? total)
            => feitos == null || total is null or 0 ? 0 : Math.Round(feitos.Value * 100.0 / total.Value);

        private async Task AbrirAsync()
        {
            if (Certificado == null)
                return;

            var parametros = new Dictionary<string, object> { ["Certificado"] = Certificado };
            await Shell.Current.GoToAsync("certificado", parametros);
        }

        private async Task BaixarPdfAsync()
        {
            var certificado = Certificado;
            if (certificado == null)
                return;

            await ExecutarAsync(async () =>
            {
                var caminho = await CertificadoPdf.BaixarAsync(certificado);
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    await AvisarAsync("Certificado", $"PDF salvo em:\n{caminho}");
            });
        }
    }
}
