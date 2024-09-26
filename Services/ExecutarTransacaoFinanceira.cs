using System;
using Microsoft.Extensions.Logging;
using TransacaoFinanceira.Data;
using TransacaoFinanceira.Domain;

namespace TransacaoFinanceira.Services
{
    public class ExecutarTransacaoFinanceira : IExecutarTransacaoFinanceira
    {
        private readonly IContasSaldoRepository _contasSaldoRepository;
        private readonly ILogger<ExecutarTransacaoFinanceira> _logger;

        public ExecutarTransacaoFinanceira(IContasSaldoRepository contasSaldoRepository, ILogger<ExecutarTransacaoFinanceira> logger)
        {
            _contasSaldoRepository = contasSaldoRepository;
            _logger = logger;
        }

        public void Transferir(int correlationId, long contaOrigem, long contaDestino, decimal valor)
        {
            try
            {
                if (contaOrigem <= 0 || contaDestino <= 0)
                {
                    _logger.LogError($"Transação número {correlationId} falhou: Número de conta inválido.");
                    return;
                }

                if (valor <= 0)
                {
                    _logger.LogError($"Transação número {correlationId} falhou: Valor de transação inválido.");
                    return;
                }

                var contaSaldoOrigem = _contasSaldoRepository.GetByConta(contaOrigem);
                var contaSaldoDestino = _contasSaldoRepository.GetByConta(contaDestino);

                if (contaSaldoOrigem == null)
                {
                    _logger.LogError($"Conta de origem {contaOrigem} não encontrada.");
                    return;
                }

                if (contaSaldoDestino == null)
                {
                    _logger.LogError($"Conta de destino {contaDestino} não encontrada.");
                    return;
                }

                if (contaSaldoOrigem.Saldo < valor)
                {
                    _logger.LogError($"Transação número {correlationId} foi cancelada por falta de saldo.");
                    return;
                }

                // Simulando uma transação atômica
                try
                {
                    contaSaldoOrigem.Saldo -= valor;
                    contaSaldoDestino.Saldo += valor;

                    _contasSaldoRepository.Update(contaSaldoOrigem);
                    _contasSaldoRepository.Update(contaSaldoDestino);

                    _logger.LogInformation($"Transação número {correlationId} foi efetivada com sucesso! " +
                                           $"Novos saldos: Conta Origem: {contaSaldoOrigem.Saldo} | Conta Destino: {contaSaldoDestino.Saldo}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao atualizar os saldos, revertendo transação.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado na transação número {correlationId}.");
                throw;
            }
        }
    }
}
