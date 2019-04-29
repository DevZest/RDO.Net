using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TCommand, TReader>
    {
        public abstract int TransactionCount { get; }

        protected abstract Transaction CurrentTransaction { get; }

        protected abstract class Transaction : ITransaction
        {
            protected internal abstract DbSession<TConnection, TCommand, TReader> GetDbSession();

            public abstract string Name { get; }

            public abstract int Level { get; }

            public abstract bool IsDisposed { get; }

            public abstract void Dispose();

            protected internal abstract Task<int> ExecuteNonQueryAsync(TCommand command, CancellationToken ct);

            protected Task<int> PerformExecuteNonQueryAsync(TCommand command, CancellationToken ct)
            {
                return GetDbSession().InternalExecuteNonQueryAsync(command, ct);
            }

            protected internal abstract Task<TReader> ExecuteReaderAsync(Model model, TCommand command, CancellationToken ct);

            protected Task<TReader> PerformExecuteReaderAsync(Model model, TCommand command, CancellationToken ct)
            {
                return GetDbSession().InternalExecuteReaderAsync(model, command, ct);
            }

            public Task CommitAsync(CancellationToken ct)
            {
                return TransactionInvoker.CommitAsync(this, ct);
            }

            protected internal abstract Task PerformCommitAsync(CancellationToken ct);

            public Task RollbackAsync(CancellationToken ct)
            {
                return TransactionInvoker.RollbackAsync(this, ct);
            }

            protected internal abstract Task PerformRollbackAsync(CancellationToken ct);
        }

        public ITransaction BeginTransaction(string name = null)
        {
            return BeginTransaction(null, name);
        }

        public ITransaction BeginTransaction(IsolationLevel? isolation, string name = null)
        {
            return TransactionInvoker.BeginTransaction(this, isolation, name);
        }

        protected internal abstract ITransaction PerformBeginTransaction(IsolationLevel? isolation, string name);
    }
}
