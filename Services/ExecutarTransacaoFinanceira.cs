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
                    _logger.LogError($"Transa��o n�mero {correlationId} falhou: N�mero de conta inv�lido.");
                    return;
                }

                if (valor <= 0)
                {
                    _logger.LogError($"Transa��o n�mero {correlationId} falhou: Valor de transa��o inv�lido.");
                    return;
                }

                var contaSaldoOrigem = _contasSaldoRepository.GetByConta(contaOrigem);
                var contaSaldoDestino = _contasSaldoRepository.GetByConta(contaDestino);

                if (contaSaldoOrigem == null)
                {
                    _logger.LogError($"Conta de origem {contaOrigem} n�o encontrada.");
                    return;
                }

                if (contaSaldoDestino == null)
                {
                    _logger.LogError($"Conta de destino {contaDestino} n�o encontrada.");
                    return;
                }

                if (contaSaldoOrigem.Saldo < valor)
                {
                    _logger.LogError($"Transa��o n�mero {correlationId} foi cancelada por falta de saldo.");
                    return;
                }

                // Simulando uma transa��o at�mica
                try
                {
                    contaSaldoOrigem.Saldo -= valor;
                    contaSaldoDestino.Saldo += valor;

                    _contasSaldoRepository.Update(contaSaldoOrigem);
                    _contasSaldoRepository.Update(contaSaldoDestino);

                    _logger.LogInformation($"Transa��o n�mero {correlationId} foi efetivada com sucesso! " +
                                           $"Novos saldos: Conta Origem: {contaSaldoOrigem.Saldo} | Conta Destino: {contaSaldoDestino.Saldo}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao atualizar os saldos, revertendo transa��o.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado na transa��o n�mero {correlationId}.");
                throw;
            }
        }
    }
}
