using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TTransaction, TCommand, TReader>
    {
        protected abstract class TransactionInvoker : AddonInvoker<IDbTransactionInterceptor<TConnection, TTransaction>>
        {
            protected TransactionInvoker(DbSession dbSession, TConnection connection, IsolationLevel? isolationLevel)
                : base(dbSession)
            {
                connection.VerifyNotNull(nameof(connection));
                Connection = connection;
                IsolationLevel = isolationLevel;
            }

            public TConnection Connection { get; private set; }

            public IsolationLevel? IsolationLevel { get; private set; }

            public TTransaction Transaction { get; private set; }

            internal void Execute(Stack<TTransaction> transactions, Action action)
            {
                InvokeBeginTransaction(transactions);
                try
                {
                    action();
                    InvokeCommit(transactions);
                }
                catch (Exception)
                {
                    InvokeRollback(transactions);
                    throw;
                }
            }

            internal async Task ExecuteAsync(Stack<TTransaction> transactions, Func<Task> action)
            {
                InvokeBeginTransaction(transactions);
                try
                {
                    await action();
                    InvokeCommit(transactions);
                }
                catch (Exception)
                {
                    InvokeRollback(transactions);
                    throw;
                }
            }

            internal async Task ExecuteAsync(Stack<TTransaction> transactions, Func<CancellationToken, Task> action, CancellationToken ct)
            {
                InvokeBeginTransaction(transactions);
                try
                {
                    await action(ct);
                    InvokeCommit(transactions);
                }
                catch (Exception)
                {
                    InvokeRollback(transactions);
                    throw;
                }
            }

            private TTransaction InvokeBeginTransaction(Stack<TTransaction> transactions)
            {
                Invoke(() => { Transaction = BeginTransaction(); }, x => x.OnBeginning(Connection, IsolationLevel, Transaction, this),
                    x =>
                    {
                        transactions.Push(Transaction);
                        x.OnBegan(Connection, IsolationLevel, Transaction, this);
                    });
                return Transaction;
            }

            private void InvokeCommit(Stack<TTransaction> transactions)
            {
                Invoke(() => Transaction.Commit(), x => x.OnCommitting(Connection, IsolationLevel, Transaction, this),
                    x =>
                    {
                        transactions.Pop();
                        x.OnCommitted(Connection, IsolationLevel, Transaction, this);
                    });
            }

            private void InvokeRollback(Stack<TTransaction> transactions)
            {
                Invoke(() => Transaction.Rollback(), x => x.OnRollingBack(Connection, IsolationLevel, Transaction, this),
                    x =>
                    {
                        transactions.Pop();
                        x.OnRolledBack(Connection, IsolationLevel, Transaction, this);
                    });
            }

            protected abstract TTransaction BeginTransaction();
        }
    }
}
