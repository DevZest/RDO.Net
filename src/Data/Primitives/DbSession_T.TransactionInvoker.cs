using DevZest.Data.Addons;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TCommand, TReader>
    {
        private sealed class TransactionInvoker : AddonInvoker<IDbTransactionInterceptor>
        {
            public static ITransaction BeginTransaction(DbSession<TConnection, TCommand, TReader> dbSession, IsolationLevel? isolationLevel, string name)
            {
                return new TransactionInvoker(dbSession).InvokeBeginTransaction(isolationLevel, name);
            }

            public static Task CommitAsync(Transaction transaction, CancellationToken ct)
            {
                return new TransactionInvoker(transaction).InvokeCommitAsync(ct);
            }

            public static Task RollbackAsync(Transaction transaction, CancellationToken ct)
            {
                return new TransactionInvoker(transaction).InvokeRollbackAsync(ct);
            }

            private TransactionInvoker(DbSession<TConnection, TCommand, TReader> dbSession)
                : base(dbSession)
            {
                DbSession = dbSession;
            }

            private TransactionInvoker(Transaction transaction)
                : base(transaction.GetDbSession())
            {
                Debug.Assert(transaction != null);
                Transaction = transaction;
            }

            private DbSession<TConnection, TCommand, TReader> DbSession { get; }
            private ITransaction BeginTransactionResult { get; set; }

            private ITransaction InvokeBeginTransaction(IsolationLevel? isolationLevel, string name)
            {
                Invoke(
                    action: () => { BeginTransactionResult = DbSession.PerformBeginTransaction(isolationLevel, name); },
                    onExecuting: x => x.OnBeginning(isolationLevel, name, this),
                    onExecuted: x => x.OnBegan(isolationLevel, name, this));
                return BeginTransactionResult;
            }

            private Transaction Transaction { get; }

            private Task InvokeCommitAsync(CancellationToken ct)
            {
                return InvokeAsync(
                    action: Transaction.PerformCommitAsync(ct),
                    onExecuting: x => x.OnCommitting(Transaction, this),
                    onExecuted: x => x.OnCommitted(Transaction, this));
            }

            private Task InvokeRollbackAsync(CancellationToken ct)
            {
                return InvokeAsync(
                    action: Transaction.PerformRollbackAsync(ct),
                    onExecuting: x => x.OnCommitting(Transaction, this),
                    onExecuted: x => x.OnCommitted(Transaction, this));
            }
        }
    }
}
