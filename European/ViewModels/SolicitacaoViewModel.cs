using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Documento do candidato na análise da coordenação: os arquivos (fotos ou PDF) e se foi aprovado ou recusado.</summary>
    public partial class DocumentoAnaliseItem : ObservableObject
    {
        public DocumentosMatricula.Exigido Exigido { get; init; } = null!;
        public string Nome => Exigido.Nome;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Enviado), nameof(Aprovado), nameof(Recusado), nameof(StatusTexto), nameof(TemObservacao), nameof(Observacao))]
        public partial DocumentoEnviado? Documento { get; set; }

        public ObservableCollection<ArquivoItem> Arquivos { get; } = new();

        public bool Enviado => Documento?.Enviado == true;
        public bool Aprovado => Documento?.Aprovado == true;
        public bool Recusado => Documento?.Recusado == true;
        public string? Observacao => Documento?.Observacao;
        public bool TemObservacao => Recusado && !string.IsNullOrWhiteSpace(Observacao);
        public string StatusTexto => Documento switch
        {
            null or { Enviado: false } => "Não enviado",
            { Aprovado: true } => "Aprovado",
            { Recusado: true } => "Recusado",
            _ => "A conferir"
        };
    }

    /// <summary>
    /// Coordenação: analisa uma solicitação de matrícula (fotos dos documentos e pagamento). "Liberar" cria a conta
    /// do aluno e a matrícula; "Devolver" manda de volta ao candidato para corrigir os documentos recusados;
    /// "Cancelar" encerra a solicitação e reembolsa o pagamento (ex.: o candidato não enviou o que faltava).
    /// </summary>
    public partial class SolicitacaoViewModel : BaseViewModel
    {
        private Curso? _curso;

        public SolicitacaoViewModel()
        {
            Title = "Solicitação de matrícula";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Email), nameof(Iniciais), nameof(CriadaEmTexto), nameof(Pago), nameof(PagamentoTexto),
            nameof(TransacaoTexto), nameof(TemPendencia), nameof(PendenciaTexto), nameof(StatusTexto), nameof(EtapaTexto), nameof(PodeLiberar),
            nameof(PodeDevolver), nameof(PodeCancelar), nameof(CancelarTexto), nameof(CursoNome), nameof(Cancelada), nameof(CanceladaTexto),
            nameof(PodeAnalisarDocumentos))]
        public partial SolicitacaoMatricula? Solicitacao { get; private set; }

        public ObservableCollection<DocumentoAnaliseItem> Documentos { get; } = new();

        public string Nome => Solicitacao?.Nome ?? string.Empty;
        public string Email => Solicitacao?.Email ?? string.Empty;
        public string Iniciais => Solicitacao?.Iniciais ?? "?";
        public string CriadaEmTexto => Solicitacao == null ? string.Empty : $"Solicitada em {Solicitacao.CriadaEm:dd/MM/yyyy HH:mm}";
        public string CursoNome => _curso?.Nome ?? "Curso não encontrado";

        public bool Pago => Solicitacao?.Pago == true;
        public string PagamentoTexto => Solicitacao is { Pago: true } s
            ? $"Pago {Formatos.Moeda(s.ValorPago ?? _curso?.Preco ?? 0)}{Parcelamento(s)} {Formatos.Pagamento(s.FormaPagamento, s.PagamentoEm)}"
            : "Pagamento ainda não realizado";
        public string TransacaoTexto => Solicitacao?.TransacaoId is { } id ? $"Transação {id}" : string.Empty;

        public bool TemPendencia => Solicitacao?.TemPendencia == true;
        public string PendenciaTexto => Solicitacao is { TemPendencia: true } s
            ? $"Devolvida em {s.PendenciaEm:dd/MM/yyyy}: {s.PendenciaMotivo}"
            : string.Empty;

        public bool Cancelada => Solicitacao?.Cancelada == true;
        public string CanceladaTexto => Solicitacao is { Cancelada: true } s
            ? $"Cancelada em {s.CanceladaEm:dd/MM/yyyy}: {s.CanceladaMotivo}" + (s.Reembolsada ? $" · reembolso {s.ReembolsoId}" : string.Empty)
            : string.Empty;

        public string StatusTexto => Solicitacao == null ? string.Empty : SolicitacaoService.StatusTexto(Solicitacao);

        public string EtapaTexto => Solicitacao?.Status switch
        {
            StatusSolicitacao.Documentos => "O candidato ainda está enviando os documentos.",
            StatusSolicitacao.Pagamento => "Documentos enviados; o candidato ainda não pagou.",
            StatusSolicitacao.EmAnalise => "Confira as fotos e o pagamento. Ainda não é usuário: a conta só é criada ao liberar.",
            StatusSolicitacao.Devolvida => "Aguardando o candidato corrigir e reenviar.",
            StatusSolicitacao.Cancelada => "Solicitação encerrada.",
            _ => string.Empty
        };

        /// <summary>Libera com tudo em ordem: em análise, pago e todos os documentos aprovados.</summary>
        public bool PodeLiberar => Solicitacao is { Status: StatusSolicitacao.EmAnalise, Pago: true, DocumentosAprovados: true } && _curso != null;
        /// <summary>Só faz sentido devolver quando há algum documento recusado (marcado com o X).</summary>
        public bool PodeDevolver => Solicitacao is { Status: StatusSolicitacao.EmAnalise } s && s.Documentos.Any(d => d.Recusado);
        public bool PodeCancelar => Solicitacao is { Cancelada: false };
        public bool PodeAnalisarDocumentos => Solicitacao?.Status == StatusSolicitacao.EmAnalise;
        public string CancelarTexto => Pago ? "Cancelar e reembolsar" : "Cancelar solicitação";

        private static string Parcelamento(SolicitacaoMatricula s)
            => s is { FormaPagamento: FormaPagamento.CartaoCredito, Parcelas: > 1 } ? $" em {s.Parcelas}x" : string.Empty;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
            => parametros.Obter<SolicitacaoMatricula>("Solicitacao") is { } solicitacao
                ? CarregarAsync(solicitacao.Id)
                : Task.CompletedTask;

        private Task CarregarAsync(string solicitacaoId) => ExecutarAsync(async () =>
        {
            var solicitacao = await SolicitacaoRepositorio.PorIdAsync(solicitacaoId);
            _curso = solicitacao == null ? null : await CursoRepositorio.PorIdAsync(solicitacao.CursoId);
            Solicitacao = solicitacao;
            OnPropertyChanged(nameof(CursoNome));
            OnPropertyChanged(nameof(PodeLiberar));

            if (solicitacao == null)
            {
                await AvisarAsync("Solicitação", "Esta solicitação não existe mais.");
                await VoltarAsync();
                return;
            }

            Documentos.Clear();
            foreach (var exigido in DocumentosMatricula.Exigidos)
                Documentos.Add(new DocumentoAnaliseItem { Exigido = exigido, Documento = solicitacao.Documento(exigido.Tipo) });

            // miniaturas depois, para a lista aparecer logo
            foreach (var item in Documentos)
                foreach (var arquivo in await ArquivoItem.DoDocumentoAsync(item.Documento))
                    item.Arquivos.Add(arquivo);
        });

        // ---- Visualizador: foto ampliada dentro do app, com zoom e giro ----

        public const double ZoomMinimo = 1;
        public const double ZoomMaximo = 5;

        [ObservableProperty]
        public partial bool MostrarVisualizador { get; private set; }

        [ObservableProperty]
        public partial string? ImagemAmpliada { get; private set; }

        [ObservableProperty]
        public partial string TituloVisualizador { get; private set; } = string.Empty;

        /// <summary>1 = a foto inteira na tela; acima disso, ampliada (dá para arrastar).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LarguraConteudo), nameof(AlturaConteudo), nameof(LimitesImagem), nameof(ZoomTexto))]
        public partial double Zoom { get; set; } = 1;

        /// <summary>Tamanho da área do visualizador (a tela informa quando muda).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LarguraConteudo), nameof(AlturaConteudo), nameof(LimitesImagem))]
        public partial Size AreaVisualizador { get; set; } = new(360, 480);

        /// <summary>Giro da foto em graus (0, 90, 180 ou 270).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LimitesImagem))]
        public partial int Rotacao { get; private set; }

        /// <summary>Área rolável: a do visualizador vezes o zoom.</summary>
        public double LarguraConteudo => AreaVisualizador.Width * Zoom;
        public double AlturaConteudo => AreaVisualizador.Height * Zoom;

        /// <summary>
        /// Posição e tamanho da foto antes de girar. Girada 90°/270°, ela é montada com largura e altura trocadas e
        /// centralizada, para que depois do giro ocupe exatamente a área rolável.
        /// </summary>
        public Rect LimitesImagem => Rotacao % 180 == 0
            ? new Rect(0, 0, LarguraConteudo, AlturaConteudo)
            : new Rect((LarguraConteudo - AlturaConteudo) / 2, (AlturaConteudo - LarguraConteudo) / 2, AlturaConteudo, LarguraConteudo);

        public string ZoomTexto => $"{Zoom * 100:0}%";

        partial void OnZoomChanged(double value)
        {
            var limitado = Math.Clamp(value, ZoomMinimo, ZoomMaximo);
            if (Math.Abs(limitado - value) > 0.0001)
                Zoom = limitado;
        }

        /// <summary>Foto: abre ampliada aqui no app. PDF: baixa e abre no leitor de PDF do aparelho.</summary>
        [RelayCommand]
        private async Task AbrirArquivoAsync(ArquivoItem? arquivo)
        {
            if (arquivo == null)
                return;

            if (arquivo.Caminho == null)
                await ExecutarAsync(arquivo.CarregarAsync);
            if (arquivo.Caminho == null)
            {
                await AvisarAsync("Documento", "Não foi possível baixar o arquivo.");
                return;
            }

            var documento = Documentos.FirstOrDefault(d => d.Exigido.Tipo == arquivo.Tipo)?.Nome ?? "Documento";
            if (arquivo.IsPdf)
            {
                try
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest(documento, new ReadOnlyFile(arquivo.Caminho, "application/pdf")));
                }
                catch (Exception ex)
                {
                    CrashLogger.Registrar(nameof(SolicitacaoViewModel), ex);
                    await AvisarAsync("Documento", "Não foi possível abrir o PDF. Verifique se há um leitor de PDF instalado.");
                }
                return;
            }

            TituloVisualizador = documento;
            ImagemAmpliada = arquivo.Caminho;
            Zoom = 1;
            Rotacao = GiroAutomatico(arquivo.Caminho);
            MostrarVisualizador = true;
        }

        /// <summary>Foto deitada numa tela em pé (ou o contrário): já abre girada, para ocupar a tela inteira.</summary>
        private int GiroAutomatico(string caminho)
        {
            if (ImagemInfo.Dimensoes(caminho) is not { } tamanho || tamanho.Width <= 0 || tamanho.Height <= 0)
                return 0;

            var fotoDeitada = tamanho.Width > tamanho.Height;
            var telaEmPe = AreaVisualizador.Height > AreaVisualizador.Width;
            return fotoDeitada == telaEmPe ? 90 : 0;
        }

        /// <summary>Gira a foto 90° no sentido horário.</summary>
        [RelayCommand]
        private void Girar() => Rotacao = (Rotacao + 90) % 360;

        [RelayCommand]
        private void AumentarZoom() => Zoom = Math.Min(ZoomMaximo, Zoom + 0.5);

        [RelayCommand]
        private void DiminuirZoom() => Zoom = Math.Max(ZoomMinimo, Zoom - 0.5);

        /// <summary>Toque duplo: alterna entre a foto inteira e 2,5x.</summary>
        [RelayCommand]
        private void AlternarZoom() => Zoom = Zoom > ZoomMinimo ? ZoomMinimo : 2.5;

        [RelayCommand]
        private void FecharVisualizador()
        {
            MostrarVisualizador = false;
            ImagemAmpliada = null;
        }

        [RelayCommand]
        private Task AprovarDocumentoAsync(DocumentoAnaliseItem? item) => AvaliarAsync(item, aprovar: true);

        [RelayCommand]
        private Task RecusarDocumentoAsync(DocumentoAnaliseItem? item) => AvaliarAsync(item, aprovar: false);

        /// <summary>O ícone de certo aprova o documento; o X recusa, pedindo o motivo que o candidato verá.</summary>
        private async Task AvaliarAsync(DocumentoAnaliseItem? item, bool aprovar)
        {
            var solicitacao = Solicitacao;
            if (item?.Documento == null || solicitacao == null || !PodeAnalisarDocumentos)
                return;
            if (aprovar ? item.Aprovado : item.Recusado)
                return;

            string? observacao = null;
            if (!aprovar)
            {
                observacao = await PerguntarAsync("Recusar", $"Motivo da recusa de \"{item.Nome}\":", "Recusar",
                    placeholder: "Ex.: foto ilegível");
                if (observacao == null)
                    return;
            }

            if (await ExecutarAsync(() => SolicitacaoService.AvaliarDocumentoAsync(solicitacao, item.Exigido.Tipo, aprovar, observacao)))
            {
                item.Documento = null;
                item.Documento = solicitacao.Documento(item.Exigido.Tipo);
                OnPropertyChanged(nameof(PodeLiberar));
                OnPropertyChanged(nameof(PodeDevolver));
            }
        }

        [RelayCommand]
        private async Task LiberarAsync()
        {
            var solicitacao = Solicitacao;
            var curso = _curso;
            if (solicitacao == null || curso == null || !PodeLiberar)
                return;

            if (!await ConfirmarAsync($"Liberar a matrícula de {solicitacao.Nome} em {curso.Nome}?", "Liberar"))
                return;

            var criada = false;
            if (!await ExecutarAsync(async () => criada = await ContaService.LiberarSolicitacaoAsync(solicitacao)))
                return;

            if (!criada)
            {
                await AvisarAsync("Liberar matrícula", "Já existe uma conta com este e-mail. Abra o aluno pela lista para liberar o curso.");
                return;
            }

            await AvisarAsync("Matrícula liberada", $"{solicitacao.Nome} já pode entrar e acessar {curso.Nome}.");
            await VoltarAsync();
        }

        /// <summary>Devolve ao candidato para corrigir; o motivo já vem com os documentos recusados.</summary>
        [RelayCommand]
        private async Task DevolverAsync()
        {
            var solicitacao = Solicitacao;
            if (solicitacao == null || !PodeDevolver)
                return;

            var recusados = solicitacao.Documentos.Where(d => d.Recusado).Select(d => DocumentosMatricula.NomeDe(d.Tipo)).ToList();
            var sugestao = recusados.Count > 0 ? $"Reenviar: {string.Join(", ", recusados)}." : string.Empty;

            var motivo = await PerguntarAsync("Devolver ao candidato", "O que o candidato precisa corrigir?", "Devolver",
                placeholder: "Ex.: reenviar o diploma legível", valorInicial: sugestao);
            if (motivo == null)
                return;

            if (string.IsNullOrWhiteSpace(motivo))
            {
                await AvisarAsync("Devolver", "Informe o que o candidato precisa corrigir.");
                return;
            }

            if (await ExecutarAsync(() => SolicitacaoService.DevolverAsync(solicitacao, motivo)))
            {
                await AvisarAsync("Solicitação devolvida", $"{solicitacao.Nome} verá o pedido de correção ao entrar no app.");
                await CarregarAsync(solicitacao.Id);
            }
        }

        /// <summary>Encerra a solicitação; se houve pagamento, o valor é reembolsado ao candidato.</summary>
        [RelayCommand]
        private async Task CancelarAsync()
        {
            var solicitacao = Solicitacao;
            if (solicitacao == null || !PodeCancelar)
                return;

            var motivo = await PerguntarAsync(CancelarTexto, "Qual o motivo do cancelamento? O candidato verá esta mensagem.", "Continuar",
                placeholder: "Ex.: documentos não reenviados no prazo");
            if (motivo == null)
                return;

            if (string.IsNullOrWhiteSpace(motivo))
            {
                await AvisarAsync("Cancelar", "Informe o motivo do cancelamento.");
                return;
            }

            var pergunta = solicitacao.Pago
                ? $"Cancelar a solicitação de {solicitacao.Nome} e reembolsar {Formatos.Moeda(solicitacao.ValorPago ?? 0)}?"
                : $"Cancelar a solicitação de {solicitacao.Nome}?";
            if (!await ConfirmarAsync(pergunta, "Cancelar solicitação"))
                return;

            if (await ExecutarAsync(() => SolicitacaoService.CancelarAsync(solicitacao, motivo, peloCandidato: false)))
            {
                await AvisarAsync("Solicitação cancelada", solicitacao.Reembolsada
                    ? $"Reembolso {solicitacao.ReembolsoId} feito para {solicitacao.Nome}."
                    : $"A solicitação de {solicitacao.Nome} foi cancelada.");
                await VoltarAsync();
            }
        }
    }
}
