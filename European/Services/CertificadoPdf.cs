using System.Globalization;
using System.Text;
using European.Models;

namespace European.Services
{
    /// <summary>
    /// Gera o PDF do certificado final (A4 paisagem) sem bibliotecas externas, usando as fontes
    /// padrão Helvetica do próprio PDF, e entrega o arquivo ao usuário.
    /// </summary>
    public static class CertificadoPdf
    {
        private const double Largura = 842;
        private const double Altura = 595;

        private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;
        private static readonly CultureInfo PtBr = new("pt-BR");

        /// <summary>Salva o PDF e o abre (Windows, na pasta Downloads) ou oferece o compartilhamento (celular). Retorna o caminho salvo.</summary>
        public static async Task<string> BaixarAsync(Certificado certificado)
        {
            var bytes = Gerar(certificado);
            var nome = $"Certificado-{NomeDeArquivo(certificado.AlunoNome)}-{certificado.CodigoVerificacao}.pdf";

#if WINDOWS
            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            Directory.CreateDirectory(pasta);
            var caminho = Path.Combine(pasta, nome);
            await File.WriteAllBytesAsync(caminho, bytes);
            await Launcher.Default.OpenAsync(new OpenFileRequest("Certificado", new ReadOnlyFile(caminho)));
#else
            var caminho = Path.Combine(FileSystem.CacheDirectory, nome);
            await File.WriteAllBytesAsync(caminho, bytes);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Certificado de conclusão",
                File = new ShareFile(caminho, "application/pdf")
            });
#endif
            return caminho;
        }

        public static byte[] Gerar(Certificado c)
        {
            var conteudo = new StringBuilder();
            var azul = (0.208, 0.145, 0.804);
            var cinza = (0.39, 0.39, 0.48);
            var escuro = (0.10, 0.10, 0.18);

            // molduras
            Cor(conteudo, azul, preenchimento: false);
            conteudo.Append("3 w 24 24 794 547 re S\n");
            conteudo.Append("0.8 w 34 34 774 527 re S\n");

            Texto(conteudo, "EUROPEAN", Fonte.Negrito, 14, 515, azul);
            Texto(conteudo, "CERTIFICADO DE CONCLUSÃO", Fonte.Negrito, 30, 462, escuro);
            Texto(conteudo, "Mestrado", Fonte.Normal, 14, 438, cinza);

            Cor(conteudo, azul, preenchimento: false);
            conteudo.Append("1 w 371 420 m 471 420 l S\n");

            Texto(conteudo, "Certificamos que", Fonte.Normal, 14, 382, cinza);
            Texto(conteudo, c.AlunoNome, Fonte.Negrito, 26, 344, escuro);
            Texto(conteudo, "concluiu com aprovação todos os módulos e avaliações do curso", Fonte.Normal, 14, 306, cinza);
            Texto(conteudo, c.CursoNome, Fonte.Negrito, 20, 274, azul);

            var detalhes = c.CargaHorariaHoras > 0
                ? $"com carga horária de {c.CargaHorariaHoras} horas e média final {c.MediaFinal.ToString("0.0", PtBr)}."
                : $"com média final {c.MediaFinal.ToString("0.0", PtBr)}.";
            Texto(conteudo, detalhes, Fonte.Normal, 13, 244, cinza);

            // assinatura e data
            Cor(conteudo, cinza, preenchimento: false);
            conteudo.Append("0.8 w 120 150 m 360 150 l S\n");
            conteudo.Append("0.8 w 482 150 m 722 150 l S\n");
            Texto(conteudo, "Coordenação Acadêmica", Fonte.Normal, 11, 134, cinza, centroX: 240);
            Texto(conteudo, c.EmitidoEm.ToString("dd 'de' MMMM 'de' yyyy", PtBr), Fonte.Normal, 11, 158, escuro, centroX: 602);
            Texto(conteudo, "Data de emissão", Fonte.Normal, 11, 134, cinza, centroX: 602);

            Texto(conteudo, $"Código de verificação: {c.CodigoVerificacao}", Fonte.Normal, 10, 62, cinza);

            return MontarDocumento(Latin1(conteudo.ToString()));
        }

        private enum Fonte { Normal, Negrito }

        private static void Texto(StringBuilder s, string texto, Fonte fonte, double tamanho, double y,
            (double R, double G, double B) cor, double centroX = Largura / 2, double larguraMaxima = 700)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return;

            // nomes muito longos diminuem a fonte para caber na página
            var largura = LarguraTexto(texto, fonte, tamanho);
            if (largura > larguraMaxima)
            {
                tamanho *= larguraMaxima / largura;
                largura = larguraMaxima;
            }

            Cor(s, cor, preenchimento: true);
            s.Append("BT /").Append(fonte == Fonte.Negrito ? "F2 " : "F1 ").Append(Num(tamanho)).Append(" Tf ")
             .Append(Num(centroX - largura / 2)).Append(' ').Append(Num(y)).Append(" Td (")
             .Append(Escapar(texto)).Append(") Tj ET\n");
        }

        private static void Cor(StringBuilder s, (double R, double G, double B) cor, bool preenchimento)
            => s.Append(Num(cor.R)).Append(' ').Append(Num(cor.G)).Append(' ').Append(Num(cor.B))
                .Append(preenchimento ? " rg\n" : " RG\n");

        private static string Num(double valor) => valor.ToString("0.###", Invariante);

        private static string Escapar(string texto)
        {
            var s = new StringBuilder(texto.Length);
            foreach (var ch in texto)
            {
                if (ch is '(' or ')' or '\\')
                    s.Append('\\');
                s.Append(ch > 0xFF ? '?' : ch);
            }
            return s.ToString();
        }

        private static double LarguraTexto(string texto, Fonte fonte, double tamanho)
        {
            var tabela = fonte == Fonte.Negrito ? LargurasNegrito : LargurasNormal;
            double total = 0;
            foreach (var ch in texto)
            {
                // letras acentuadas têm a mesma largura da letra base
                var baseCh = ch.ToString().Normalize(NormalizationForm.FormD)[0];
                total += baseCh >= 32 && baseCh <= 126 ? tabela[baseCh - 32] : 556;
            }
            return total * tamanho / 1000;
        }

        private static byte[] MontarDocumento(byte[] conteudo)
        {
            using var saida = new MemoryStream();
            var offsets = new List<long>();

            void Escrever(string texto) { var b = Latin1(texto); saida.Write(b, 0, b.Length); }
            void Objeto(string corpo) { offsets.Add(saida.Position); Escrever($"{offsets.Count} 0 obj\n{corpo}\nendobj\n"); }

            Escrever("%PDF-1.4\n%âãÏÓ\n");
            Objeto("<< /Type /Catalog /Pages 2 0 R >>");
            Objeto("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
            Objeto($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {Num(Largura)} {Num(Altura)}] " +
                   "/Resources << /Font << /F1 5 0 R /F2 6 0 R >> >> /Contents 4 0 R >>");

            offsets.Add(saida.Position);
            Escrever($"4 0 obj\n<< /Length {conteudo.Length} >>\nstream\n");
            saida.Write(conteudo, 0, conteudo.Length);
            Escrever("\nendstream\nendobj\n");

            Objeto("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
            Objeto("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

            var xref = saida.Position;
            var tabela = new StringBuilder($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets)
                tabela.Append(offset.ToString("0000000000", Invariante)).Append(" 00000 n \n");
            tabela.Append($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            Escrever(tabela.ToString());

            return saida.ToArray();
        }

        private static byte[] Latin1(string texto) => Encoding.Latin1.GetBytes(texto);

        private static string NomeDeArquivo(string nome)
        {
            var invalidos = Path.GetInvalidFileNameChars();
            var limpo = new string(nome.Where(ch => !invalidos.Contains(ch)).ToArray()).Trim().Replace(' ', '-');
            return string.IsNullOrEmpty(limpo) ? "aluno" : limpo;
        }

        // Larguras (em 1/1000 do tamanho da fonte) dos caracteres 32..126 das fontes padrão do PDF.
        private static readonly int[] LargurasNormal =
        {
            278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,
            1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,
            667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,
            333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
            556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584
        };

        private static readonly int[] LargurasNegrito =
        {
            278, 333, 474, 556, 556, 889, 722, 238, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 333, 333, 584, 584, 584, 611,
            975, 722, 722, 722, 722, 667, 611, 778, 722, 278, 556, 722, 611, 833, 722, 778,
            667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 333, 278, 333, 584, 556,
            333, 556, 611, 556, 611, 556, 333, 611, 611, 278, 278, 556, 278, 889, 611, 611,
            611, 611, 389, 556, 333, 611, 556, 778, 556, 556, 500, 389, 280, 389, 584
        };
    }
}
