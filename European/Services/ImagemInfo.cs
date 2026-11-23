namespace European.Services
{
    /// <summary>
    /// Lê largura e altura de uma foto direto do cabeçalho do arquivo (PNG ou JPEG), sem carregar a imagem.
    /// Em JPEG de celular, considera a orientação EXIF: a foto "deitada no arquivo" que o celular marca para girar.
    /// </summary>
    public static class ImagemInfo
    {
        /// <summary>Tamanho como a foto aparece na tela, ou null se o formato não for reconhecido.</summary>
        public static Size? Dimensoes(string caminho)
        {
            try
            {
                using var arquivo = File.OpenRead(caminho);
                using var leitor = new BinaryReader(arquivo);
                var cabecalho = leitor.ReadBytes(8);

                if (cabecalho.Length == 8 && cabecalho[0] == 0x89 && cabecalho[1] == 'P' && cabecalho[2] == 'N' && cabecalho[3] == 'G')
                {
                    arquivo.Position = 16;
                    return new Size(LerInt32BE(leitor), LerInt32BE(leitor));
                }

                if (cabecalho.Length >= 2 && cabecalho[0] == 0xFF && cabecalho[1] == 0xD8)
                {
                    arquivo.Position = 2;
                    return Jpeg(arquivo, leitor);
                }
            }
            catch (Exception ex) when (ex is IOException or EndOfStreamException or UnauthorizedAccessException)
            {
                CrashLogger.Registrar(nameof(ImagemInfo), ex);
            }

            return null;
        }

        private static Size? Jpeg(Stream arquivo, BinaryReader leitor)
        {
            var orientacao = 1;
            while (arquivo.Position < arquivo.Length)
            {
                if (leitor.ReadByte() != 0xFF)
                    return null;

                var marcador = leitor.ReadByte();
                while (marcador == 0xFF)
                    marcador = leitor.ReadByte();

                var tamanho = LerUInt16BE(leitor);
                var inicio = arquivo.Position;

                if (marcador == 0xE1)
                    orientacao = OrientacaoExif(leitor, tamanho - 2) ?? orientacao;

                // SOF0..SOF15 (menos DHT, JPG e DAC): altura e largura da imagem
                if (marcador is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC)
                {
                    arquivo.Position = inicio + 1;
                    var altura = LerUInt16BE(leitor);
                    var largura = LerUInt16BE(leitor);

                    // orientações 5 a 8: o celular guarda a foto deitada e manda girar 90°
                    return orientacao is >= 5 and <= 8 ? new Size(altura, largura) : new Size(largura, altura);
                }

                arquivo.Position = inicio + tamanho - 2;
            }

            return null;
        }

        /// <summary>Lê a tag de orientação (0x0112) do bloco EXIF.</summary>
        private static int? OrientacaoExif(BinaryReader leitor, int tamanho)
        {
            var dados = leitor.ReadBytes(tamanho);
            if (dados.Length < 14 || dados[0] != 'E' || dados[1] != 'x' || dados[2] != 'i' || dados[3] != 'f')
                return null;

            const int tiff = 6;
            var littleEndian = dados[tiff] == 'I';
            int U16(int i) => littleEndian ? dados[i] | dados[i + 1] << 8 : dados[i] << 8 | dados[i + 1];
            int U32(int i) => littleEndian
                ? dados[i] | dados[i + 1] << 8 | dados[i + 2] << 16 | dados[i + 3] << 24
                : dados[i] << 24 | dados[i + 1] << 16 | dados[i + 2] << 8 | dados[i + 3];

            var ifd = tiff + U32(tiff + 4);
            if (ifd + 2 > dados.Length)
                return null;

            var entradas = U16(ifd);
            for (var e = 0; e < entradas; e++)
            {
                var pos = ifd + 2 + e * 12;
                if (pos + 12 > dados.Length)
                    break;
                if (U16(pos) == 0x0112)
                    return U16(pos + 8);
            }

            return null;
        }

        private static int LerInt32BE(BinaryReader leitor)
        {
            var b = leitor.ReadBytes(4);
            return b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3];
        }

        private static int LerUInt16BE(BinaryReader leitor)
        {
            var b = leitor.ReadBytes(2);
            return b[0] << 8 | b[1];
        }
    }
}
