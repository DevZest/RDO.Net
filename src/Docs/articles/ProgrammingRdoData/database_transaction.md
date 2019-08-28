# Database Transaction

Transactions ensure that data-oriented resources are not permanently updated unless all operations within the transactional unit complete successfully. By combining a set of related operations into a unit that either completely succeeds or completely fails, you can simplify error recovery and make your application more reliable.

## Implicit Transaction using Transaction Scope

The [TransactionScope](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscope) class provides a simple way to mark a block of code as participating in a transaction, without requiring you to interact with the transaction itself. A transaction scope can select and manage the ambient transaction automatically. Due to its ease of use and efficiency, it is recommended that you use the TransactionScope class when developing a transaction application.

Since your `Db` class encapsulates underlying ADO.Net database connections and commands, implicit transaction using TransactionScope is fully supported. For more information, please refer to [Implementing an Implicit Transaction using Transaction Scope](https://docs.microsoft.com/en-us/dotnet/framework/data/transactions/implementing-an-implicit-transaction-using-transaction-scope).

## Native Database Transaction

Each ADO.Net database provider also provides native database transaction. For example, SqlTransaction is provided by ADO.Net SQL Server provider. RDO.Data encapsulates native database transaction by providing <xref:DevZest.Data.Primitives.DbSession`3.BeginTransaction*> API to start a transaction by returning a <xref:DevZest.Data.ITransaction> object, which you can use to further call <xref:DevZest.Data.ITransaction.CommitAsync*> or <xref:DevZest.Data.ITransaction.RollbackAsync*>:

# [C#](#tab/cs)

```cs
using (var transaction = BeginTransaction())
{
    ...
    await transaction.CommitAsync(ct);
}
```

# [VB.Net](#tab/vb)

```vb
Using transaction = BeginTransaction()
    ...
    Await transaction.CommitAsync(ct)
End Using
```

***

<xref:DevZest.Data.ITransaction> can be nested. How nested transaction handled is database session provider specific. For example, <xref:DevZest.Data.SqlServer.SqlSession> handles nested transaction as SQL Server save point.
