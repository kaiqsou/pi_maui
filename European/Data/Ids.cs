using System.Buffers.Binary;
using System.Security.Cryptography;

namespace European.Data
{
    /// <summary>
    /// Gera ids no formato do ObjectId do MongoDB (24 caracteres hexadecimais: data + aleatório + contador),
    /// sem depender do driver do MongoDB. Usado pelo app e pelo Seed.
    /// </summary>
    public static class Ids
    {
        private static readonly byte[] Aleatorio = RandomNumberGenerator.GetBytes(5);
        private static int _contador = RandomNumberGenerator.GetInt32(0, 0xFFFFFF);

        public static string Novo()
        {
            Span<byte> bytes = stackalloc byte[12];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Aleatorio.CopyTo(bytes[4..]);

            var contador = Interlocked.Increment(ref _contador) & 0xFFFFFF;
            bytes[9] = (byte)(contador >> 16);
            bytes[10] = (byte)(contador >> 8);
            bytes[11] = (byte)contador;

            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
