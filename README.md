# TransacaoFinanceira

Case para refatoração

Passos a implementar:

1. [✅] Corrija o que for necessário para resolver os erros de compilação.
2. [✅] Execute o programa para avaliar a saida, identifique e corrija o motivo de algumas transações estarem sendo canceladas mesmo com saldo positivo e outras sem saldo sendo efetivadas.
3. [✅] Aplique o code review e refatore conforme as melhores práticas (SOLID,Patterns,etc).
4. [✅] Implemente os testes unitários que julgar efetivo.
5. [✅] Crie um repositório no GitHub e compartilhe o link respondendo o último e-mail.

Obs: Voce é livre para implementar na linguagem de sua preferência desde que respeite as funcionalidades e saídas existentes, além de aplicar os conceitos solicitados.

## Code Review

Ao executar a solução pela primeira vez, surgiram alguns problemas os quais precisaram ser resolvidos para que a saída fosse devidamente exibida no console. Abaixo segue um overview de comos eles foramm resolvidos.

1. Erro ao executar o comando `dotnet restore`:

```
Danimar@Danimar-Desktop MINGW64 ~/source/repos/TransacaoFinanceira (feature/compilation-error-fix)
$ dotnet restore
  Determinando os projetos a serem restaurados...
C:\Program Files\dotnet\sdk\8.0.401\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.EolTargetFrameworks.targets(32,5): war
ning NETSDK1138: não há mais suporte para a estrutura de destino 'net5.0' e ela não receberá atualizações de segurança  
no futuro. Confira https://aka.ms/dotnet-core-support para obter mais informações sobre a política de suporte. [C:\User 
s\Danimar\source\repos\TransacaoFinanceira\TransacaoFinanceira.csproj]
C:\Program Files\dotnet\sdk\8.0.401\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.EolTargetFrameworks.targets(32,5): war
ning NETSDK1138: não há mais suporte para a estrutura de destino 'net5.0' e ela não receberá atualizações de segurança  
no futuro. Confira https://aka.ms/dotnet-core-support para obter mais informações sobre a política de suporte. [C:\User 
s\Danimar\source\repos\TransacaoFinanceira\TransacaoFinanceira.csproj]
  C:\Users\Danimar\source\repos\TransacaoFinanceira\TransacaoFinanceira.csproj restaurado (em 112 ms).
```

A mensagem claramente diz que a solução é atualizar o framework alvo para uma versão mais recente e suportada, neste caso o .NET 8, pois é compatível com os requisitos.

Para corrigir este problema, precisamos atualizar a versão do framework para a 8.0 no arquivo TransacaoFinanceira.csproj:
```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

</Project>
```

---

2. Erro ao executar o comando `dotnet build`:
```
Danimar@Danimar-Desktop MINGW64 ~/Desktop/TransacaoFinanceira (feature/compilation-error-fix)
$ dotnet build
  Determinando os projetos a serem restaurados...
  Todos os projetos estão atualizados para restauração.
C:\Users\Danimar\Desktop\TransacaoFinanceira\Program.cs(72,48): error CS1503: Argumento 1: não é possível converter de "uint" para "int" [C:\Users\Danimar\Desktop\TransacaoFinanceira\TransacaoFinanceira.csproj]
C:\Users\Danimar\Desktop\TransacaoFinanceira\Program.cs(15,30): error CS0826: Não foi encontrado nenhum tipo melhor para a matriz do tipo implícita [C:\Users\Danimar\Desktop\TransacaoFinanceira\TransacaoFinanceira.csproj]

FALHA da compilação.

C:\Users\Danimar\Desktop\TransacaoFinanceira\Program.cs(72,48): error CS1503: Argumento 1: não é possível converter de "uint" para "int" [C:\Users\Danimar\Desktop\TransacaoFinanceira\TransacaoFinanceira.csproj]
C:\Users\Danimar\Desktop\TransacaoFinanceira\Program.cs(15,30): error CS0826: Não foi encontrado nenhum tipo melhor para a matriz do tipo implícita [C:\Users\Danimar\Desktop\TransacaoFinanceira\TransacaoFinanceira.csproj]
    0 Aviso(s)
    2 Erro(s)

Tempo Decorrido 00:00:03.20
```

---

3. Erro de tipagem:

O principal problema estava relacionado aos tipos de dados utilizados para armazenar os números das contas. Alguns números excediam o limite do tipo int, resultando em erros de conversão. A solução foi alterar o tipo das contas para long (int64) e garantir que os números grandes fossem marcados com o sufixo L (long).

Alterações:
```
// Atualizado de int para long para suportar números grandes
public long conta { get; set; }
```

E as transações foram atualizadas para garantir que os números grandes fossem reconhecidos como long:
```
var TRANSACOES = new[]
{
    new { correlation_id = 1, datetime = "09/09/2023 14:15:00", conta_origem = 938485762L, conta_destino = 2147483649L, VALOR = 150 },
};
```

4. Problemas com cancelamento de transações válidas:

O uso de Parallel.ForEach estava causando _race conditions_, onde várias threads acessavam e modificavam os saldos simultaneamente, resultando em comportamentos inconsistentes. A solução foi usar blocos `lock` para garantir que apenas uma thread por vez pudesse acessar e modificar o saldo de uma conta.

```
public void transferir(int correlation_id, long conta_origem, long conta_destino, decimal valor)
{
	// Garante que uma transação por vez altere o saldo de uma conta
    lock (this)  
    {
        contas_saldo conta_saldo_origem = getSaldo<contas_saldo>(conta_origem);
        if (conta_saldo_origem.saldo < valor)
        {
            Console.WriteLine("Transacao numero {0} foi cancelada por falta de saldo", correlation_id);
        }
        else
        {
            contas_saldo conta_saldo_destino = getSaldo<contas_saldo>(conta_destino);
            conta_saldo_origem.saldo -= valor;
            conta_saldo_destino.saldo += valor;
            Console.WriteLine("Transacao numero {0} foi efetivada com sucesso! Novos saldos: Conta Origem: {1} | Conta Destino: {2}", correlation_id, conta_saldo_origem.saldo, conta_saldo_destino.saldo);
        }
    }
}
```

Primeira saída das transações, porém com inconsistências:
```
Danimar@Danimar-Desktop MINGW64 ~/source/repos/TransacaoFinanceira (feature/compilation-error-fix)
$ dotnet run
Transacao numero 2 foi cancelada por falta de saldo
Transacao numero 1 foi efetivada com sucesso! Novos saldos: Conta Origem: 30 | Conta Destino: 150
Transacao numero 8 foi efetivada com sucesso! Novos saldos: Conta Origem: 588 | Conta Destino: 5050
Transacao numero 6 foi efetivada com sucesso! Novos saldos: Conta Origem: 738 | Conta Destino: 1249
Transacao numero 4 foi cancelada por falta de saldo
Transacao numero 7 foi efetivada com sucesso! Novos saldos: Conta Origem: -14 | Conta Destino: 194
Transacao numero 5 foi cancelada por falta de saldo
Transacao numero 3 foi efetivada com sucesso! Novos saldos: Conta Origem: 100 | Conta Destino: 1578
```

Segunda saída das transações, sem problemas, utilizando Thread-safe:
```
Danimar@Danimar-Desktop MINGW64 ~/source/repos/TransacaoFinanceira (feature/correcao-validacao-saldo-transacao)
$ dotnet run
Transacao numero 6 foi efetivada com sucesso! Novos saldos: Conta Origem: 738 | Conta Destino: 1249
Transacao numero 7 foi efetivada com sucesso! Novos saldos: Conta Origem: 136 | Conta Destino: 44
Transacao numero 1 foi cancelada por falta de saldo
Transacao numero 5 foi cancelada por falta de saldo
Transacao numero 8 foi efetivada com sucesso! Novos saldos: Conta Origem: 588 | Conta Destino: 5050
Transacao numero 4 foi cancelada por falta de saldo
Transacao numero 3 foi efetivada com sucesso! Novos saldos: Conta Origem: 100 | Conta Destino: 1578
Transacao numero 2 foi cancelada por falta de saldo
```