using TransacaoFinanceira.Domain;

namespace TransacaoFinanceira.Data
{
    public interface IContasSaldoRepository
    {
        ContasSaldo GetByConta(long conta);
        bool Update(ContasSaldo contaSaldo);
    }
}
