namespace TransacaoFinanceira.Services
{
    public interface IExecutarTransacaoFinanceira
    {
        void Transferir(int correlationId, long contaOrigem, long contaDestino, decimal valor);
    }
}
