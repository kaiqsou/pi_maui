using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>O certificado único do mestrado: emitido ao concluir todos os módulos com aprovação pela média em todas as matérias.</summary>
    public partial class CertificadoViewModel : BaseViewModel
    {
        public CertificadoViewModel()
        {
            Title = "Certificados";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PossuiCertificado), nameof(Pendente), nameof(EmissaoTexto), nameof(MediaTexto))]
        public partial Certificado? Certificado { get; private set; }

        public bool PossuiCertificado => Certificado != null;
        public bool Pendente => Certificado == null;
        public string EmissaoTexto => Certificado?.EmitidoEm.ToString("dd/MM/yyyy") ?? string.Empty;
        public string MediaTexto => Certificado?.MediaFinal.ToString("0.0") ?? string.Empty;

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
            nameof(MateriasPercentual), nameof(ModulosOk), nameof(MateriasOk))]
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

            if (aluno != null)
            {
                var matricula = (await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id)).FirstOrDefault();
                if (matricula?.Curso != null)
                {
                    cursoNome = matricula.Curso.Nome;
                    situacao = await ProgressoService.SituacaoAsync(matricula, matricula.Curso);

                    // emite agora se o aluno já cumpriu os requisitos (ex.: aprovado em uma prova depois da última aula)
                    if (situacao.Apto && !await CertificadoRepositorio.ExisteParaMatriculaAsync(matricula.Id))
                        await ProgressoService.AtualizarAsync(matricula);
                }

                certificado = (await CertificadoRepositorio.DoAlunoAsync(aluno.Id)).FirstOrDefault();
                if (certificado != null)
                {
                    if (string.IsNullOrEmpty(cursoNome))
                        cursoNome = certificado.CursoNome;

                    // mesma data que libera o login (Matricula.AcessoAte)
                    var acessoAte = certificado.AcessoAulasAte(matricula);
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
