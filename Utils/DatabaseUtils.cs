using Microsoft.Data.Sqlite;
using System;

namespace TransacaoFinanceira.Utils
{
    public static class DatabaseUtils
    {
        private static string connectionString = "Data Source=contas.db";

        public static void EnsureDatabaseCreated()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Cria a tabela Contas se ela ainda não existir, com a coluna "Nome"
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Contas (
                        Conta INTEGER PRIMARY KEY,
                        Nome TEXT NOT NULL,  -- Adicionando o nome do titular da conta
                        PIN TEXT NOT NULL,
                        Saldo DECIMAL NOT NULL
                    );";
                tableCmd.ExecuteNonQuery();

                Console.WriteLine("Banco de dados e tabela 'Contas' garantidos.");

                // Inserir contas de exemplo, se desejar (apenas na primeira execução)
                InsertExampleAccounts(connection);
            }
        }

        private static void InsertExampleAccounts(SqliteConnection connection)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Contas (Conta, Nome, PIN, Saldo)
                SELECT 12345, 'Danimar', '1234', 180
                WHERE NOT EXISTS (SELECT 1 FROM Contas WHERE Conta = 12345);
                
                INSERT INTO Contas (Conta, Nome, PIN, Saldo)
                SELECT 54321, 'Jaqueline', '4321', 1200
                WHERE NOT EXISTS (SELECT 1 FROM Contas WHERE Conta = 54321);

                INSERT INTO Contas (Conta, Nome, PIN, Saldo)
                SELECT 98765, 'Diego', '1111', 0
                WHERE NOT EXISTS (SELECT 1 FROM Contas WHERE Conta = 98765);";

            insertCmd.ExecuteNonQuery();
        }
    }
}
