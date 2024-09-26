using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransacaoFinanceira.Utils;

namespace TransacaoFinanceira
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configuração do serviço de logging
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            // Chama o utilitário para garantir que o banco de dados e a tabela estejam criados
            DatabaseUtils.EnsureDatabaseCreated();

            string connectionString = "Data Source=contas.db";

            bool sair = false;

            // Solicitar o número da conta e PIN do usuário uma única vez
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                Console.Write("Digite o número da conta: ");
                long conta = long.Parse(Console.ReadLine());

                Console.Write("Digite o PIN: ");
                string pin = Console.ReadLine();

                // Verifica se a conta existe e se o PIN está correto
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Saldo, Nome FROM Contas WHERE Conta = $conta AND PIN = $pin";
                cmd.Parameters.AddWithValue("$conta", conta);
                cmd.Parameters.AddWithValue("$pin", pin);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        logger?.LogError("Conta não encontrada ou PIN incorreto.");
                        return;
                    }

                    // Exibe o saldo e nome do usuário
                    reader.Read();
                    decimal saldo = reader.GetDecimal(0);
                    string nome = reader.GetString(1);
                    Console.WriteLine($"\nOlá {nome}!");
                    Console.WriteLine($"Saldo da conta: R$ {saldo}\n");

                    while (!sair)
                    {
                        Console.WriteLine("Selecione a opção que deseja:");
                        Console.WriteLine("1. Realizar transferência");
                        Console.WriteLine("2. Sair");
                        string opcao = Console.ReadLine();

                        if (opcao == "2")
                        {
                            Console.WriteLine("Saindo do sistema. Até logo!");
                            break;
                        }

                        if (opcao == "1")
                        {
                            long contaDestino = 0;
                            bool validacaoContaDestino = false;

                            // Garantir que a conta de destino não seja a mesma que a conta de origem
                            do
                            {
                                Console.Write("Digite o número da conta de destino: ");
                                contaDestino = long.Parse(Console.ReadLine());

                                if (conta == contaDestino)
                                {
                                    logger.LogError("Não é possível transferir para a mesma conta.");
                                    Console.WriteLine("Por favor, digite uma conta de destino diferente.");
                                }
                                else
                                {
                                    validacaoContaDestino = true;
                                }
                            }
                            while (!validacaoContaDestino); // Continua solicitando até que uma conta válida seja inserida

                            Console.Write("Digite o valor da transferência: ");
                            decimal valor = decimal.Parse(Console.ReadLine());

                            if (valor > saldo)
                            {
                                logger.LogError("Saldo insuficiente para realizar a transferência.");
                                continue;
                            }

                            Console.Write("Confirme seu PIN para concluir a transferência: ");
                            string pinConfirmacao = Console.ReadLine();

                            if (pinConfirmacao != pin)
                            {
                                logger.LogError("PIN incorreto. Transferência não realizada.");
                                continue;
                            }

                            // Transação de transferência atômica
                            var transaction = connection.BeginTransaction();
                            try
                            {
                                // IsolationLevel
                                // .Race Condition.

                                // Débito na conta de origem
                                var cmdUpdateOrigem = connection.CreateCommand();
                                cmdUpdateOrigem.CommandText = "UPDATE Contas SET Saldo = Saldo - $valor WHERE Conta = $conta";
                                cmdUpdateOrigem.Parameters.AddWithValue("$valor", valor);
                                cmdUpdateOrigem.Parameters.AddWithValue("$conta", conta);
                                cmdUpdateOrigem.ExecuteNonQuery();

                                // Crédito na conta de destino
                                var cmdUpdateDestino = connection.CreateCommand();
                                cmdUpdateDestino.CommandText = "UPDATE Contas SET Saldo = Saldo + $valor WHERE Conta = $contaDestino";
                                cmdUpdateDestino.Parameters.AddWithValue("$valor", valor);
                                cmdUpdateDestino.Parameters.AddWithValue("$contaDestino", contaDestino);
                                cmdUpdateDestino.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LogInformation($"Transferência de R$ {valor} realizada com sucesso de {conta} para {contaDestino}");

                                // Atualiza o saldo após a transferência
                                saldo -= valor;
                                Console.WriteLine($"Saldo atualizado: R$ {saldo}\n");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                logger.LogError(ex, "Erro ao realizar a transferência.");
                            }
                        }
                    }
                }
            }
        }
    }
}
