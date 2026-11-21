namespace European.Enums
{
    /// <summary>Etapas da solicitação de matrícula, do cadastro até a coordenação liberar (ou cancelar).</summary>
    public enum StatusSolicitacao
    {
        /// <summary>Cadastro feito; falta enviar as fotos dos documentos.</summary>
        Documentos,

        /// <summary>Documentos enviados; falta pagar.</summary>
        Pagamento,

        /// <summary>Documentos e pagamento enviados; aguardando a análise da coordenação.</summary>
        EmAnalise,

        /// <summary>A coordenação devolveu ao candidato (documento recusado ou outra pendência) para ele corrigir e reenviar.</summary>
        Devolvida,

        /// <summary>Cancelada pela coordenação ou pelo candidato (desistência); o pagamento, se houve, foi reembolsado.</summary>
        Cancelada
    }
}
