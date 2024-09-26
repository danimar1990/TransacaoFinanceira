using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransacaoFinanceira
{
    class Program
    {
        static void Main(string[] args)
        {
            // Alteração efetuada: Os parâmetros 'conta_origem' e 'conta_destino' devem ser do tipo 'long', então acresccentamos 'L' ao final do número
            var TRANSACOES = new[]
            {
                new { correlation_id = 1, datetime = "09/09/2023 14:15:00", conta_origem = 938485762L, conta_destino = 2147483649L, VALOR = 150 },
                new { correlation_id = 2, datetime = "09/09/2023 14:15:05", conta_origem = 2147483649L, conta_destino = 210385733L, VALOR = 149 },
                new { correlation_id = 3, datetime = "09/09/2023 14:15:29", conta_origem = 347586970L, conta_destino = 238596054L, VALOR = 1100 },
                new { correlation_id = 4, datetime = "09/09/2023 14:17:00", conta_origem = 675869708L, conta_destino = 210385733L, VALOR = 5300 },
                new { correlation_id = 5, datetime = "09/09/2023 14:18:00", conta_origem = 238596054L, conta_destino = 674038564L, VALOR = 1489 },
                new { correlation_id = 6, datetime = "09/09/2023 14:18:20", conta_origem = 573659065L, conta_destino = 563856300L, VALOR = 49 },
                new { correlation_id = 7, datetime = "09/09/2023 14:19:00", conta_origem = 938485762L, conta_destino = 2147483649L, VALOR = 44 },
                new { correlation_id = 8, datetime = "09/09/2023 14:19:01", conta_origem = 573659065L, conta_destino = 675869708L, VALOR = 150 }
            };

            executarTransacaoFinanceira executor = new executarTransacaoFinanceira();
            Parallel.ForEach(TRANSACOES, item =>
            {
                executor.transferir(item.correlation_id, item.conta_origem, item.conta_destino, item.VALOR);
            });
        }
    }

    class executarTransacaoFinanceira : acessoDados
    {
        // Alteração efetuada: Trocamos 'int' por 'long' nos parâmetros 'conta_origem' e 'conta_destino'.
        public void transferir(int correlation_id, long conta_origem, long conta_destino, decimal valor)
        {
            // Observação: A chamada do método 'getSaldo' já está sendo feita com 'long' em vez de 'int' para a conta de origem.
            contas_saldo conta_saldo_origem = getSaldo<contas_saldo>(conta_origem);
            if (conta_saldo_origem.saldo < valor)
            {
                Console.WriteLine("Transacao numero {0} foi cancelada por falta de saldo", correlation_id);
            }
            else
            {
                // Observação: A chamada do método 'getSaldo' já está sendo feita com 'long' em vez de 'int' para a conta de destino.
                contas_saldo conta_saldo_destino = getSaldo<contas_saldo>(conta_destino);
                conta_saldo_origem.saldo -= valor;
                conta_saldo_destino.saldo += valor;
                Console.WriteLine("Transacao numero {0} foi efetivada com sucesso! Novos saldos: Conta Origem: {1} | Conta Destino: {2}", correlation_id, conta_saldo_origem.saldo, conta_saldo_destino.saldo);
            }
        }
    }

    class contas_saldo
    {
        // Alteração efetuada: Alteramos o parâmetro 'conta' no tipo da conta para utilizar 'long'.
        public contas_saldo(long conta, decimal valor)
        {
            this.conta = conta;
            this.saldo = valor;
        }

        // Alteração efetuada: Aqui também alteramos para utilizar 'long'.
        public long conta { get; set; }
        public decimal saldo { get; set; }
    }

    class acessoDados
    {
        private List<contas_saldo> TABELA_SALDOS;

        public acessoDados()
        {
            // Alteração efetuada: adicionando objetos diretamente quando a lista é criada, reduzindo a quantidade de código redundante
            TABELA_SALDOS = new List<contas_saldo>
            {
                new contas_saldo(938485762L, 180),
                new contas_saldo(347586970L, 1200),
                new contas_saldo(2147483649L, 0),
                new contas_saldo(675869708L, 4900),
                new contas_saldo(238596054L, 478),
                new contas_saldo(573659065L, 787),
                new contas_saldo(210385733L, 10),
                new contas_saldo(674038564L, 400),
                new contas_saldo(563856300L, 1200)
            };
        }

        // Alteração efetuada: Alteramos o parâmetro para utilizar 'long' e não mais 'int'.
        public T getSaldo<T>(long id)
        {
            return (T)Convert.ChangeType(TABELA_SALDOS.Find(x => x.conta == id), typeof(T));
        }

        public bool atualizar<T>(T dado)
        {
            try
            {
                contas_saldo item = (dado as contas_saldo);
                TABELA_SALDOS.RemoveAll(x => x.conta == item.conta);
                TABELA_SALDOS.Add(dado as contas_saldo);
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
