using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Um documento exigido, com os arquivos enviados (fotos ou PDF) e a situação na análise da coordenação.</summary>
    public partial class DocumentoItem : ObservableObject
    {
        public DocumentosMatricula.Exigido Exigido { get; init; } = null!;
        public string Nome => Exigido.Nome;
        public string Dica => Exigido.Dica;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Enviado), nameof(Recusado), nameof(StatusTexto), nameof(Observacao), nameof(TemObservacao), nameof(PodeAdicionar))]
        public partial DocumentoEnviado? Documento { get; set; }

        public ObservableCollection<ArquivoItem> Arquivos { get; } = new();

        public bool Enviado => Documento?.Enviado == true;
        public bool Recusado => Documento?.Recusado == true;
        public string? Observacao => Documento?.Observacao;
        public bool TemObservacao => Recusado && !string.IsNullOrWhiteSpace(Observacao);
        public bool PodeAdicionar => (Documento?.Arquivos.Count ?? 0) < DocumentoEnviado.MaximoArquivos;
        public string StatusTexto => Documento switch
        {
            null or { Enviado: false } => "Pendente",
            { Recusado: true } => "Recusado: corrija e envie de novo",
            _ => Documento.Arquivos.Count == 1 ? "1 arquivo enviado" : $"{Documento.Arquivos.Count} arquivos enviados"
        };

        public async Task AtualizarAsync(DocumentoEnviado? documento)
        {
            Documento = null;
            Documento = documento;
            Arquivos.Clear();
            foreach (var arquivo in await ArquivoItem.DoDocumentoAsync(documento))
                Arquivos.Add(arquivo);
        }
    }

    /// <summary>
    /// Candidato envia os documentos exigidos para o mestrado: um ou mais arquivos por documento (ex.: frente e verso),
    /// em foto ou PDF. É a primeira etapa depois do cadastro e também onde ele corrige os documentos recusados quando
    /// a coordenação devolve a solicitação.
    /// </summary>
    public partial class DocumentosViewModel : BaseViewModel
    {
        private static readonly FilePickerFileType TiposAceitos = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = new[] { ".jpg", ".jpeg", ".png", ".heic", ".webp", ".pdf" },
            [DevicePlatform.Android] = new[] { "image/*", "application/pdf" },
            [DevicePlatform.iOS] = new[] { "public.image", "com.adobe.pdf" },
            [DevicePlatform.MacCatalyst] = new[] { "public.image", "com.adobe.pdf" }
        });

        public DocumentosViewModel()
        {
            Title = "Documentos";
        }

        public ObservableCollection<DocumentoItem> Itens { get; } = new();

        [ObservableProperty]
        public partial string CursoNome { get; private set; } = string.Empty;

        /// <summary>Correção depois de a coordenação devolver: mostra o motivo.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemPendencia))]
        public partial string PendenciaTexto { get; private set; } = string.Empty;

        public bool TemPendencia => !string.IsNullOrEmpty(PendenciaTexto);

        [ObservableProperty]
        public partial string ResumoTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string BotaoContinuarTexto { get; private set; } = "Continuar para o pagamento";

        private SolicitacaoMatricula? Solicitacao => SessionService.SolicitacaoAtual;

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var solicitacao = await Candidatura.RecarregarAsync();
            if (solicitacao == null)
            {
                await AcessoService.SairAsync();
                return;
            }

            // a coordenação pode ter mudado a etapa (ex.: cancelou) enquanto a tela estava fechada
            if (solicitacao.Status is not (StatusSolicitacao.Documentos or StatusSolicitacao.Devolvida))
            {
                await Candidatura.IrParaEtapaAsync(solicitacao);
                return;
            }

            CursoNome = (await CursoRepositorio.PorIdAsync(solicitacao.CursoId))?.Nome ?? string.Empty;
            PendenciaTexto = solicitacao.TemPendencia ? solicitacao.PendenciaMotivo! : string.Empty;
            BotaoContinuarTexto = solicitacao.Pago ? "Reenviar para análise" : "Continuar para o pagamento";

            Itens.Clear();
            foreach (var exigido in DocumentosMatricula.Exigidos)
                Itens.Add(new DocumentoItem { Exigido = exigido });
            AtualizarResumo();

            // miniaturas por último, para a lista aparecer logo
            foreach (var item in Itens)
                await item.AtualizarAsync(solicitacao.Documento(item.Exigido.Tipo));
            AtualizarResumo();
        });

        private void AtualizarResumo()
        {
            var prontos = Itens.Count(i => i.Enviado && !i.Recusado);
            ResumoTexto = $"{prontos} de {Itens.Count} documentos enviados";
        }

        /// <summary>Acrescenta um arquivo ao documento (foto da câmera, imagem ou PDF).</summary>
        [RelayCommand]
        private async Task AdicionarAsync(DocumentoItem? item)
        {
            var solicitacao = Solicitacao;
            if (item == null || solicitacao == null || IsBusy)
                return;

            if (!item.PodeAdicionar)
            {
                await AvisarAsync("Documentos", $"Envie no máximo {DocumentoEnviado.MaximoArquivos} arquivos por documento.");
                return;
            }

            var arquivo = await EscolherArquivoAsync();
            if (arquivo == null)
                return;

            await ExecutarAsync(async () =>
            {
                await using var conteudo = await arquivo.OpenReadAsync();
                if (conteudo.CanSeek && conteudo.Length > SolicitacaoService.TamanhoMaximoArquivo)
                {
                    await AvisarAsync("Documentos", "Arquivo muito grande. Envie até 10 MB.");
                    return;
                }

                await SolicitacaoService.AdicionarArquivoAsync(solicitacao, item.Exigido.Tipo, arquivo.FileName, conteudo);
                await item.AtualizarAsync(solicitacao.Documento(item.Exigido.Tipo));
                AtualizarResumo();
            });
        }

        [RelayCommand]
        private async Task RemoverAsync(ArquivoItem? arquivo)
        {
            var solicitacao = Solicitacao;
            if (arquivo == null || solicitacao == null || IsBusy)
                return;

            if (!await ConfirmarAsync("Remover este arquivo?", "Remover"))
                return;

            await ExecutarAsync(async () =>
            {
                await SolicitacaoService.RemoverArquivoAsync(solicitacao, arquivo.Tipo, arquivo.Arquivo.ArquivoId);
                if (Itens.FirstOrDefault(i => i.Exigido.Tipo == arquivo.Tipo) is { } item)
                    await item.AtualizarAsync(solicitacao.Documento(arquivo.Tipo));
                AtualizarResumo();
            });
        }

        /// <summary>No celular: tirar foto ou escolher arquivo (imagem ou PDF); no computador, o seletor de arquivos.</summary>
        private static async Task<FileResult?> EscolherArquivoAsync()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported && DeviceInfo.Platform != DevicePlatform.WinUI)
                {
                    var escolha = await Shell.Current.DisplayActionSheetAsync("Enviar documento", "Cancelar", null, "Tirar foto", "Escolher arquivo");
                    if (escolha == "Tirar foto")
                        return await MediaPicker.Default.CapturePhotoAsync();
                    if (escolha != "Escolher arquivo")
                        return null;
                }

                return await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Foto ou PDF do documento", FileTypes = TiposAceitos });
            }
            catch (PermissionException)
            {
                await Shell.Current.DisplayAlertAsync("Documentos", "Permita o acesso à câmera ou aos arquivos para enviar o documento.", "OK");
                return null;
            }
        }

        [RelayCommand]
        private async Task ContinuarAsync()
        {
            var solicitacao = Solicitacao;
            if (solicitacao == null)
                return;

            string? erro = null;
            if (!await ExecutarAsync(async () => erro = await SolicitacaoService.ConcluirDocumentosAsync(solicitacao)))
                return;

            if (erro != null)
            {
                await AvisarAsync("Documentos", erro);
                return;
            }

            if (solicitacao.Status == StatusSolicitacao.EmAnalise)
                await AvisarAsync("Documentos", "Documentos reenviados para a análise.");
            await Candidatura.IrParaEtapaAsync(solicitacao);
        }

        /// <summary>Só na correção: a desistência existe depois que a solicitação chegou à análise.</summary>
        [RelayCommand]
        private async Task DesistirAsync()
        {
            if (Solicitacao is { Status: StatusSolicitacao.Devolvida } solicitacao)
                await Candidatura.DesistirAsync(solicitacao);
        }

        [RelayCommand]
        private Task Sair() => AcessoService.SairAsync();
    }
}
