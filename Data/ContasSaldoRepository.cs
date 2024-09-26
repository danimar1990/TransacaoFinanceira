using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TransacaoFinanceira.Domain;

namespace TransacaoFinanceira.Data
{
    public class ContasSaldoRepository : IContasSaldoRepository
    {
        private readonly List<ContasSaldo> _tabelaSaldos;
        private readonly ILogger<ContasSaldoRepository> _logger;

        public ContasSaldoRepository(ILogger<ContasSaldoRepository> logger)
        {
            _logger = logger;

            _tabelaSaldos = new List<ContasSaldo>
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

        public ContasSaldo GetByConta(long conta)
        {
            try
            {
                var saldo = _tabelaSaldos.Find(x => x.Conta == conta);
                if (saldo == null)
                {
                    _logger.LogWarning($"Conta {conta} não encontrada.");
                }
                return saldo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter saldo da conta {conta}.");
                throw;
            }
        }

        public bool Update(ContasSaldo contaSaldo)
        {
            try
            {
                var index = _tabelaSaldos.FindIndex(x => x.Conta == contaSaldo.Conta);
                if (index == -1)
                {
                    _logger.LogWarning($"Conta {contaSaldo.Conta} não encontrada para atualização.");
                    return false;
                }

                _tabelaSaldos[index] = contaSaldo;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar saldo da conta {contaSaldo.Conta}.");
                throw;
            }
        }
    }
}
