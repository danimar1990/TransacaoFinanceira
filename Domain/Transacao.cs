namespace TransacaoFinanceira.Domain
{
    public class Transacao
    {
        public int CorrelationId { get; set; }
        public string DateTime { get; set; }
        public long ContaOrigem { get; set; }
        public long ContaDestino { get; set; }
        public decimal Valor { get; set; }
    }
}
