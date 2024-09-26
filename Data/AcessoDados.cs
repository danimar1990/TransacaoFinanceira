using System;
using System.Collections.Generic;
using TransacaoFinanceira.Domain;

namespace TransacaoFinanceira.Data
{
    public class AcessoDados : IAcessoDados
    {
        private List<ContasSaldo> TabelaSaldos { get; set; }

        public AcessoDados()
        {
            TabelaSaldos = new List<ContasSaldo>
            {
                new ContasSaldo(938485762, 180),
                new ContasSaldo(347586970, 1200),
                new ContasSaldo(2147483649, 0),
                new ContasSaldo(675869708, 4900),
                new ContasSaldo(238596054, 478),
                new ContasSaldo(573659065, 787),
                new ContasSaldo(210385733, 10),
                new ContasSaldo(674038564, 400),
                new ContasSaldo(563856300, 1200)
            };
        }

        public ContasSaldo GetSaldo(long id)
        {
            return TabelaSaldos.Find(x => x.Conta == id);
        }

        public bool Atualizar(ContasSaldo dado)
        {
            try
            {
                TabelaSaldos.RemoveAll(x => x.Conta == dado.Conta);
                TabelaSaldos.Add(dado);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
