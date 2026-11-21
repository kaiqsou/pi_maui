using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace European.Services
{
    /// <summary>
    /// Código Pix "copia e cola" no padrão BR Code do Banco Central (EMV QRCPS): campos identificados por
    /// ID + tamanho + valor, terminando no CRC16 do conteúdo. O mesmo texto vira o QR Code da tela de pagamento.
    /// </summary>
    public static class PixService
    {
        private const string Recebedor = "EUROPEAN EDUCACAO";
        private const string Cidade = "SAO PAULO";

        /// <summary>Cobrança da matrícula: chave do recebedor, valor e um identificador da transação ligado à solicitação.</summary>
        public static string GerarCodigo(string solicitacaoId, decimal valor)
        {
            var conta = Campo("00", "BR.GOV.BCB.PIX") + Campo("01", Chave(solicitacaoId));
            var txid = new string(solicitacaoId.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            txid = txid.Length > 25 ? txid[^25..] : txid;

            var conteudo = Campo("00", "01")
                           + Campo("01", "12")
                           + Campo("26", conta)
                           + Campo("52", "0000")
                           + Campo("53", "986")
                           + Campo("54", valor.ToString("0.00", CultureInfo.InvariantCulture))
                           + Campo("58", "BR")
                           + Campo("59", Recebedor)
                           + Campo("60", Cidade)
                           + Campo("62", Campo("05", txid))
                           + "6304";

            return conteudo + Crc16(conteudo).ToString("X4");
        }

        private static string Campo(string id, string valor) => id + valor.Length.ToString("00") + valor;

        /// <summary>Chave aleatória (formato EVP) da cobrança, fixa para cada solicitação.</summary>
        private static string Chave(string solicitacaoId)
            => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(solicitacaoId)).AsSpan(0, 16)).ToString();

        /// <summary>CRC16-CCITT (polinômio 0x1021, início 0xFFFF), como pede o BR Code.</summary>
        private static ushort Crc16(string texto)
        {
            ushort crc = 0xFFFF;
            foreach (var b in Encoding.UTF8.GetBytes(texto))
            {
                crc ^= (ushort)(b << 8);
                for (var i = 0; i < 8; i++)
                    crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
            }
            return crc;
        }
    }
}
