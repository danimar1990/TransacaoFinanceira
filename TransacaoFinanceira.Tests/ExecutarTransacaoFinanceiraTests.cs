using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using TransacaoFinanceira.Data;
using TransacaoFinanceira.Domain;
using TransacaoFinanceira.Services;
using System;

namespace TransacaoFinanceira.Tests
{
    [TestFixture]
    public class ExecutarTransacaoFinanceiraTests
    {
        private Mock<IContasSaldoRepository> _repoMock;
        private Mock<ILogger<ExecutarTransacaoFinanceira>> _loggerMock;
        private ExecutarTransacaoFinanceira _executor;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IContasSaldoRepository>();
            _loggerMock = new Mock<ILogger<ExecutarTransacaoFinanceira>>();
            _executor = new ExecutarTransacaoFinanceira(_repoMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Transferir_ContaOrigemInvalida_RegistroDeErro()
        {
            // Act
            _executor.Transferir(1, -1, 12345, 100);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once, "Deveria logar erro ao passar conta origem inválida.");
        }

        [Test]
        public void Transferir_ContaDestinoInvalida_RegistroDeErro()
        {
            // Act
            _executor.Transferir(1, 12345, -1, 100);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once, "Deveria logar erro ao passar conta destino inválida.");
        }

        [Test]
        public void Transferir_ValorInvalido_RegistroDeErro()
        {
            // Act
            _executor.Transferir(1, 12345, 54321, -100);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once, "Deveria logar erro ao passar valor de transação inválido.");
        }

        [Test]
        public void Transferir_ContaOrigemNaoEncontrada_RegistroDeErro()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByConta(12345)).Returns((ContasSaldo)null);

            // Act
            _executor.Transferir(1, 12345, 54321, 100);

            // Assert
            _loggerMock.Verify(x => x.LogError("Conta de origem 12345 não encontrada."), Times.Once);
        }

        [Test]
        public void Transferir_ContaDestinoNaoEncontrada_RegistroDeErro()
        {
            // Arrange
            var contaOrigem = new ContasSaldo(12345, 500);
            _repoMock.Setup(r => r.GetByConta(12345)).Returns(contaOrigem);
            _repoMock.Setup(r => r.GetByConta(54321)).Returns((ContasSaldo)null);

            // Act
            _executor.Transferir(1, 12345, 54321, 100);

            // Assert
            _loggerMock.Verify(x => x.LogError("Conta de destino 54321 não encontrada."), Times.Once);
        }

        [Test]
        public void Transferir_SaldoInsuficiente_RegistroDeErro()
        {
            // Arrange
            var contaOrigem = new ContasSaldo(12345, 50);
            var contaDestino = new ContasSaldo(54321, 500);

            _repoMock.Setup(r => r.GetByConta(12345)).Returns(contaOrigem);
            _repoMock.Setup(r => r.GetByConta(54321)).Returns(contaDestino);

            // Act
            _executor.Transferir(1, 12345, 54321, 100);

            // Assert
            _loggerMock.Verify(x => x.LogError("Transação número 1 foi cancelada por falta de saldo."), Times.Once);
        }

        [Test]
        public void Transferir_Sucesso_AtualizaSaldosELogaInformacao()
        {
            // Arrange
            var contaOrigem = new ContasSaldo(12345, 500);
            var contaDestino = new ContasSaldo(54321, 100);

            _repoMock.Setup(r => r.GetByConta(12345)).Returns(contaOrigem);
            _repoMock.Setup(r => r.GetByConta(54321)).Returns(contaDestino);

            // Act
            _executor.Transferir(1, 12345, 54321, 100);

            // Assert
						NUnit.Framework.Assert.AreEqual(400, contaOrigem.Saldo, "Saldo da conta de origem deveria ser debitado.");
						NUnit.Framework.Assert.AreEqual(200, contaDestino.Saldo, "Saldo da conta de destino deveria ser creditado.");

            _repoMock.Verify(r => r.Update(contaOrigem), Times.Once);
            _repoMock.Verify(r => r.Update(contaDestino), Times.Once);

            _loggerMock.Verify(x => x.LogInformation(
                "Transação número 1 foi efetivada com sucesso! Novos saldos: Conta Origem: 400 | Conta Destino: 200"), Times.Once);
        }

        [Test]
        public void Transferir_FalhaNaAtualizacao_RegistroDeErro()
        {
            // Arrange
            var contaOrigem = new ContasSaldo(12345, 500);
            var contaDestino = new ContasSaldo(54321, 100);

            _repoMock.Setup(r => r.GetByConta(12345)).Returns(contaOrigem);
            _repoMock.Setup(r => r.GetByConta(54321)).Returns(contaDestino);

            _repoMock.Setup(r => r.Update(It.IsAny<ContasSaldo>())).Throws(new Exception("Falha ao atualizar"));

            // Act & Assert
            Assert.Throws<Exception>(() => _executor.Transferir(1, 12345, 54321, 100), "Falha ao atualizar");

            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), "Falha ao atualizar os saldos, revertendo transação."), Times.Once);
        }
    }
}
