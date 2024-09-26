using TransacaoFinanceira.Domain;

namespace TransacaoFinanceira.Data
{
    public interface IAcessoDados
    {
        ContasSaldo GetSaldo(long id);
        bool Atualizar(ContasSaldo dado);
    }
}
