using System.Text;

namespace European.Services
{
    /// <summary>
    /// Gerador de QR Code (ISO/IEC 18004) em modo byte, com correção de erro nível M, sem bibliotecas externas.
    /// Baseado no algoritmo de referência de Project Nayuki (licença MIT). Devolve a matriz de módulos
    /// (true = escuro), que o controle QrCodeView desenha.
    /// </summary>
    public static class QrCode
    {
        // Nível M: códigos de correção por bloco e número de blocos, por versão (índice 0 não usado).
        private static readonly int[] EccPorBloco =
        {
            -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26,
            26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28
        };

        private static readonly int[] Blocos =
        {
            -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16,
            17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49
        };

        private const int FormatoNivelM = 0;
        private const int Mascara = 0;

        public static bool[,] Gerar(string texto)
        {
            var dados = Encoding.UTF8.GetBytes(texto);

            // menor versão em que os dados cabem
            var versao = 1;
            for (; versao <= 40; versao++)
            {
                var bitsContagem = versao <= 9 ? 8 : 16;
                if (4 + bitsContagem + dados.Length * 8 <= CodewordsDeDados(versao) * 8)
                    break;
            }

            if (versao > 40)
                throw new ArgumentException("Texto grande demais para um QR Code.");

            var codewords = MontarDados(dados, versao);
            var matriz = new Matriz(versao);
            matriz.DesenharPadroes();
            matriz.DesenharCodewords(IntercalarComCorrecao(codewords, versao));
            matriz.AplicarMascara(Mascara);
            matriz.DesenharFormato(Mascara);
            return matriz.Modulos;
        }

        private static byte[] MontarDados(byte[] dados, int versao)
        {
            var bits = new List<bool>();
            void Anexar(int valor, int quantidade)
            {
                for (var i = quantidade - 1; i >= 0; i--)
                    bits.Add(((valor >> i) & 1) != 0);
            }

            Anexar(0x4, 4); // modo byte
            Anexar(dados.Length, versao <= 9 ? 8 : 16);
            foreach (var b in dados)
                Anexar(b, 8);

            var capacidade = CodewordsDeDados(versao) * 8;
            Anexar(0, Math.Min(4, capacidade - bits.Count)); // terminador
            Anexar(0, (8 - bits.Count % 8) % 8);
            for (var preenchimento = 0xEC; bits.Count < capacidade; preenchimento ^= 0xEC ^ 0x11)
                Anexar(preenchimento, 8);

            var resultado = new byte[bits.Count / 8];
            for (var i = 0; i < bits.Count; i++)
                if (bits[i])
                    resultado[i >> 3] |= (byte)(1 << (7 - (i & 7)));
            return resultado;
        }

        private static byte[] IntercalarComCorrecao(byte[] dados, int versao)
        {
            var numBlocos = Blocos[versao];
            var eccLen = EccPorBloco[versao];
            var brutos = ModulosDeDados(versao) / 8;
            var curtos = numBlocos - brutos % numBlocos;
            var tamanhoCurto = brutos / numBlocos;

            var divisor = Divisor(eccLen);
            var blocos = new List<byte[]>();
            for (int i = 0, k = 0; i < numBlocos; i++)
            {
                var tamanho = tamanhoCurto - eccLen + (i < curtos ? 0 : 1);
                var dat = dados.Skip(k).Take(tamanho).ToList();
                k += tamanho;
                var ecc = Resto(dat, divisor);
                if (i < curtos)
                    dat.Add(0);
                blocos.Add(dat.Concat(ecc).ToArray());
            }

            var resultado = new List<byte>();
            for (var i = 0; i < blocos[0].Length; i++)
                for (var j = 0; j < blocos.Count; j++)
                    if (i != tamanhoCurto - eccLen || j >= curtos)
                        resultado.Add(blocos[j][i]);
            return resultado.ToArray();
        }

        private static byte[] Divisor(int grau)
        {
            var resultado = new byte[grau];
            resultado[grau - 1] = 1;
            var raiz = 1;
            for (var i = 0; i < grau; i++)
            {
                for (var j = 0; j < grau; j++)
                {
                    resultado[j] = (byte)Multiplicar(resultado[j], raiz);
                    if (j + 1 < grau)
                        resultado[j] ^= resultado[j + 1];
                }
                raiz = Multiplicar(raiz, 0x02);
            }
            return resultado;
        }

        private static byte[] Resto(IEnumerable<byte> dados, byte[] divisor)
        {
            var resultado = new byte[divisor.Length];
            foreach (var b in dados)
            {
                var fator = b ^ resultado[0];
                Array.Copy(resultado, 1, resultado, 0, resultado.Length - 1);
                resultado[^1] = 0;
                for (var i = 0; i < resultado.Length; i++)
                    resultado[i] ^= (byte)Multiplicar(divisor[i], fator);
            }
            return resultado;
        }

        private static int Multiplicar(int x, int y)
        {
            var z = 0;
            for (var i = 7; i >= 0; i--)
            {
                z = (z << 1) ^ ((z >> 7) * 0x11D);
                z ^= ((y >> i) & 1) * x;
            }
            return z & 0xFF;
        }

        private static int ModulosDeDados(int versao)
        {
            var resultado = (16 * versao + 128) * versao + 64;
            if (versao >= 2)
            {
                var numAlinhamento = versao / 7 + 2;
                resultado -= (25 * numAlinhamento - 10) * numAlinhamento - 55;
                if (versao >= 7)
                    resultado -= 36;
            }
            return resultado;
        }

        private static int CodewordsDeDados(int versao) => ModulosDeDados(versao) / 8 - EccPorBloco[versao] * Blocos[versao];

        private sealed class Matriz
        {
            private readonly int _versao;
            private readonly int _tamanho;
            private readonly bool[,] _funcao;

            public bool[,] Modulos { get; }

            public Matriz(int versao)
            {
                _versao = versao;
                _tamanho = versao * 4 + 17;
                Modulos = new bool[_tamanho, _tamanho];
                _funcao = new bool[_tamanho, _tamanho];
            }

            private void Definir(int x, int y, bool escuro)
            {
                Modulos[y, x] = escuro;
                _funcao[y, x] = true;
            }

            public void DesenharPadroes()
            {
                for (var i = 0; i < _tamanho; i++)
                {
                    Definir(6, i, i % 2 == 0);
                    Definir(i, 6, i % 2 == 0);
                }

                Localizador(3, 3);
                Localizador(_tamanho - 4, 3);
                Localizador(3, _tamanho - 4);

                var posicoes = PosicoesAlinhamento();
                var n = posicoes.Length;
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (!(i == 0 && j == 0 || i == 0 && j == n - 1 || i == n - 1 && j == 0))
                            Alinhamento(posicoes[i], posicoes[j]);

                DesenharFormato(0);
                DesenharVersao();
            }

            private void Localizador(int x, int y)
            {
                for (var dy = -4; dy <= 4; dy++)
                    for (var dx = -4; dx <= 4; dx++)
                    {
                        var distancia = Math.Max(Math.Abs(dx), Math.Abs(dy));
                        int xx = x + dx, yy = y + dy;
                        if (xx >= 0 && xx < _tamanho && yy >= 0 && yy < _tamanho)
                            Definir(xx, yy, distancia != 2 && distancia != 4);
                    }
            }

            private void Alinhamento(int x, int y)
            {
                for (var dy = -2; dy <= 2; dy++)
                    for (var dx = -2; dx <= 2; dx++)
                        Definir(x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
            }

            private int[] PosicoesAlinhamento()
            {
                if (_versao == 1)
                    return Array.Empty<int>();

                var n = _versao / 7 + 2;
                var passo = _versao == 32 ? 26 : (_versao * 4 + n * 2 + 1) / (n * 2 - 2) * 2;
                var resultado = new int[n];
                resultado[0] = 6;
                for (int i = n - 1, pos = _tamanho - 7; i >= 1; i--, pos -= passo)
                    resultado[i] = pos;
                return resultado;
            }

            public void DesenharFormato(int mascara)
            {
                var dados = FormatoNivelM << 3 | mascara;
                var resto = dados;
                for (var i = 0; i < 10; i++)
                    resto = (resto << 1) ^ ((resto >> 9) * 0x537);
                var bits = (dados << 10 | resto) ^ 0x5412;
                bool Bit(int i) => ((bits >> i) & 1) != 0;

                for (var i = 0; i <= 5; i++)
                    Definir(8, i, Bit(i));
                Definir(8, 7, Bit(6));
                Definir(8, 8, Bit(7));
                Definir(7, 8, Bit(8));
                for (var i = 9; i < 15; i++)
                    Definir(14 - i, 8, Bit(i));

                for (var i = 0; i < 8; i++)
                    Definir(_tamanho - 1 - i, 8, Bit(i));
                for (var i = 8; i < 15; i++)
                    Definir(8, _tamanho - 15 + i, Bit(i));
                Definir(8, _tamanho - 8, true);
            }

            private void DesenharVersao()
            {
                if (_versao < 7)
                    return;

                var resto = _versao;
                for (var i = 0; i < 12; i++)
                    resto = (resto << 1) ^ ((resto >> 11) * 0x1F25);
                var bits = _versao << 12 | resto;

                for (var i = 0; i < 18; i++)
                {
                    var bit = ((bits >> i) & 1) != 0;
                    int a = _tamanho - 11 + i % 3, b = i / 3;
                    Definir(a, b, bit);
                    Definir(b, a, bit);
                }
            }

            public void DesenharCodewords(byte[] dados)
            {
                var i = 0;
                for (var direita = _tamanho - 1; direita >= 1; direita -= 2)
                {
                    if (direita == 6)
                        direita = 5;
                    for (var vertical = 0; vertical < _tamanho; vertical++)
                        for (var j = 0; j < 2; j++)
                        {
                            var x = direita - j;
                            var subindo = ((direita + 1) & 2) == 0;
                            var y = subindo ? _tamanho - 1 - vertical : vertical;
                            if (!_funcao[y, x] && i < dados.Length * 8)
                            {
                                Modulos[y, x] = ((dados[i >> 3] >> (7 - (i & 7))) & 1) != 0;
                                i++;
                            }
                        }
                }
            }

            public void AplicarMascara(int mascara)
            {
                for (var y = 0; y < _tamanho; y++)
                    for (var x = 0; x < _tamanho; x++)
                    {
                        var inverter = mascara switch
                        {
                            0 => (x + y) % 2 == 0,
                            1 => y % 2 == 0,
                            2 => x % 3 == 0,
                            3 => (x + y) % 3 == 0,
                            4 => (x / 3 + y / 2) % 2 == 0,
                            5 => x * y % 2 + x * y % 3 == 0,
                            6 => (x * y % 2 + x * y % 3) % 2 == 0,
                            _ => ((x + y) % 2 + x * y % 3) % 2 == 0
                        };
                        if (inverter && !_funcao[y, x])
                            Modulos[y, x] = !Modulos[y, x];
                    }
            }
        }
    }
}
