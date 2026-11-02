using European.Enums;

namespace European.Services
{
    public static class PagamentoService
    {
        public static string Nome(FormaPagamento forma) => forma switch
        {
            FormaPagamento.CartaoCredito => "cartão de crédito",
            FormaPagamento.CartaoDebito => "cartão de débito",
            _ => "Pix"
        };
    }
}
