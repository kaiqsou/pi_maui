using System.Globalization;
using European.Enums;

namespace European.Services
{
    /// <summary>Textos exibidos nas telas: moeda, nomes, categorias e formas de pagamento.</summary>
    public static class Formatos
    {
        public static readonly CultureInfo PtBr = new("pt-BR");

        public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

        public static string Preco(decimal valor) => valor <= 0 ? "Gratuito" : Moeda(valor);

        public static string PrimeiroNome(string? nome)
            => (nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        public static string Saudacao(string? nome) => $"Olá, {PrimeiroNome(nome)}";

        public static string Categoria(CategoriaEnum categoria) => categoria switch
        {
            CategoriaEnum.Educacao => "Educação",
            CategoriaEnum.Engenharia => "Engenharia",
            CategoriaEnum.Gestao => "Gestão",
            CategoriaEnum.Direito => "Direito",
            CategoriaEnum.Saude => "Saúde",
            _ => categoria.ToString()
        };

        public static string FormaPagamento(FormaPagamento forma) => forma switch
        {
            Enums.FormaPagamento.CartaoCredito => "cartão de crédito",
            Enums.FormaPagamento.CartaoDebito => "cartão de débito",
            _ => "Pix"
        };

        /// <summary>Ex.: "via Pix em 05/11/2026 14:30" (vazio se ainda não pagou).</summary>
        public static string Pagamento(FormaPagamento? forma, DateTime? quando, string formatoData = "dd/MM/yyyy HH:mm")
            => forma is { } f && quando is { } q ? $"via {FormaPagamento(f)} em {q.ToString(formatoData)}" : string.Empty;

        /// <summary>Lê um número digitado (aceita vírgula ou ponto); devolve o padrão se for inválido.</summary>
        public static double LerNumero(string? texto, double padrao)
            => LerNumero(texto) ?? padrao;

        /// <summary>Lê um número digitado (aceita vírgula ou ponto); null se vazio ou inválido.</summary>
        public static double? LerNumero(string? texto)
            => double.TryParse((texto ?? string.Empty).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)
                ? valor
                : null;

        /// <summary>Número para um campo editável: "7.5", "10" (sem zeros à direita).</summary>
        public static string Numero(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>Letra da alternativa: 0 → "A", 1 → "B"...</summary>
        public static string Letra(int indice) => ((char)('A' + indice)).ToString();

        /// <summary>"Peso 2" ao lado da questão (vazio quando o peso é o normal, 1).</summary>
        public static string PesoQuestao(double peso) => peso == 1 ? string.Empty : $"Peso {peso:0.#}";

        /// <summary>Duração em mm:ss (ou h:mm:ss a partir de 1 hora).</summary>
        public static string Tempo(double segundos)
            => TimeSpan.FromSeconds(segundos).ToString(segundos >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");
    }
}
