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
            protected abstract DbSession<TConnection, TCommand, TReader> GetDbSession();

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

            public abstract Task CommitAsync(CancellationToken ct);

            public abstract Task RollbackAsync(CancellationToken ct);
        }

        public abstract ITransaction BeginTransaction(string name = null);
    }
}
