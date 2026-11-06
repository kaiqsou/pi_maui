using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>O certificado único do mestrado: emitido ao concluir todos os módulos com aprovação pela média em todas as matérias.</summary>
    public partial class CertificadoViewModel : BaseViewModel
    {
        private Matricula? _matricula;

        public CertificadoViewModel()
        {
            Title = "Certificados";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiCertificado), nameof(Pendente), nameof(EmissaoTexto), nameof(PodeGerar), nameof(PendenteTexto))]
        public partial Certificado? Certificado { get; private set; }

        public bool PossuiCertificado => Certificado != null;
        public bool Pendente => Certificado == null;
        public string EmissaoTexto => Certificado?.EmitidoEm.ToString("dd/MM/yyyy") ?? string.Empty;

        /// <summary>Requisitos cumpridos e certificado ainda não gerado: mostra o botão "Gerar certificado".</summary>
        public bool PodeGerar => Pendente && Situacao?.Apto == true && _matricula?.Status is StatusMatricula.Ativa or StatusMatricula.Concluida;

        public string PendenteTexto => PodeGerar
            ? "Você cumpriu todos os requisitos. Gere agora o seu certificado de conclusão."
            : _matricula?.Status == StatusMatricula.Trancada
                ? "Sua matrícula está trancada. Reabra a matrícula para continuar o curso."
                : "O certificado fica disponível após concluir todos os módulos e ser aprovado pela média em todas as matérias.";

        [ObservableProperty]
        public partial string CursoNome { get; private set; } = string.Empty;

        /// <summary>Até quando o aluno pode reassistir as aulas depois de concluir o curso.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiValidade))]
        public partial string ValidadeTexto { get; private set; } = string.Empty;

        public bool PossuiValidade => !string.IsNullOrEmpty(ValidadeTexto);

        /// <summary>Quanto falta para o certificado (módulos concluídos e matérias aprovadas).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiSituacao), nameof(ModulosTexto), nameof(MateriasTexto), nameof(ModulosPercentual),
            nameof(MateriasPercentual), nameof(ModulosOk), nameof(MateriasOk), nameof(PodeGerar), nameof(PendenteTexto))]
        private partial ProgressoService.SituacaoCertificado? Situacao { get; set; }

        public bool PossuiSituacao => Situacao != null;
        public string ModulosTexto => Situacao == null ? string.Empty : $"{Situacao.ModulosConcluidos} de {Situacao.TotalModulos}";
        public string MateriasTexto => Situacao == null ? string.Empty : $"{Situacao.MateriasAprovadas} de {Situacao.TotalMateriasComProva}";
        public double ModulosPercentual => Percentual(Situacao?.ModulosConcluidos, Situacao?.TotalModulos);
        public double MateriasPercentual => Situacao?.TotalMateriasComProva == 0 ? 100 : Percentual(Situacao?.MateriasAprovadas, Situacao?.TotalMateriasComProva);
        public bool ModulosOk => Situacao != null && Situacao.TotalModulos > 0 && Situacao.ModulosConcluidos == Situacao.TotalModulos;
        public bool MateriasOk => Situacao != null && Situacao.MateriasAprovadas == Situacao.TotalMateriasComProva;

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            Certificado? certificado = null;
            ProgressoService.SituacaoCertificado? situacao = null;
            var cursoNome = string.Empty;
            var validade = string.Empty;
            _matricula = null;

            if (aluno != null)
            {
                // a matrícula em curso; se o acesso às aulas já venceu, a última concluída (o certificado ainda pode ser gerado)
                var matriculas = await MatriculaRepositorio.DoAlunoAsync(aluno.Id);
                _matricula = matriculas.FirstOrDefault(m => m.AcessoAulasVigente)
                             ?? matriculas.FirstOrDefault(m => m.Status is StatusMatricula.Concluida or StatusMatricula.Trancada);
                if (_matricula?.Curso != null)
                {
                    cursoNome = _matricula.Curso.Nome;
                    situacao = await ProgressoService.SituacaoAsync(_matricula, _matricula.Curso);
                    certificado = await CertificadoRepositorio.DaMatriculaAsync(_matricula.Id);
                }

                certificado ??= (await CertificadoRepositorio.DoAlunoAsync(aluno.Id)).FirstOrDefault();
                if (certificado != null)
                {
                    if (string.IsNullOrEmpty(cursoNome))
                        cursoNome = certificado.CursoNome;

                    // mesma data que libera o login (Matricula.AcessoAte)
                    var acessoAte = certificado.AcessoAulasAte(_matricula);
                    validade = acessoAte > DateTime.Now
                        ? $"Você pode reassistir as aulas até {acessoAte:dd/MM/yyyy}."
                        : $"O acesso às aulas terminou em {acessoAte:dd/MM/yyyy}.";
                }
            }

            Situacao = situacao;
            CursoNome = cursoNome;
            ValidadeTexto = validade;
            Certificado = certificado;
        });

        /// <summary>O aluno gera o próprio certificado quando cumpriu todos os requisitos.</summary>
        [RelayCommand]
        private async Task GerarAsync()
        {
            if (_matricula == null || !PodeGerar)
                return;

            Certificado? emitido = null;
            if (!await ExecutarAsync(async () => emitido = await ProgressoService.EmitirCertificadoAsync(_matricula)))
                return;

            if (emitido == null)
            {
                await AvisarAsync("Certificado", "Ainda há requisitos pendentes para emitir o certificado.");
                return;
            }

            await AoAparecerAsync();
            await AvisarAsync("Certificado emitido", $"Parabéns! Seu certificado foi gerado com o código {emitido.CodigoVerificacao}.");
        }

        private static double Percentual(int? feitos, int? total)
            => feitos == null || total is null or 0 ? 0 : Math.Round(feitos.Value * 100.0 / total.Value);

        [RelayCommand]
        private Task Abrir()
            => Certificado == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.Certificado, ("Certificado", Certificado));

        [RelayCommand]
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
