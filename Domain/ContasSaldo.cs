namespace TransacaoFinanceira.Domain
{
    public class ContasSaldo
    {
        public long Conta { get; set; }
        public decimal Saldo { get; set; }

        public ContasSaldo(long conta, decimal saldo)
        {
            Conta = conta;
            Saldo = saldo;
        }
    }
}
