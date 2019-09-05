using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TCommand, TReader>
    {
        /// <summary>
        /// Gets the count of nested transactions.
        /// </summary>
        public abstract int TransactionCount { get; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        protected abstract Transaction CurrentTransaction { get; }

        /// <summary>
        /// Represents a database transaction.
        /// </summary>
        protected abstract class Transaction : ITransaction
        {
            /// <summary>
            /// Gets the database session which owns this transaction.
            /// </summary>
            /// <returns></returns>
            protected internal abstract DbSession<TConnection, TCommand, TReader> GetDbSession();

            /// <summary>
            /// Gets the name of the transaction.
            /// </summary>
            public abstract string Name { get; }

            /// <summary>
            /// Gets the nested level of the transaction.
            /// </summary>
            public abstract int Level { get; }

            /// <summary>
            /// Gets a value indicates whether this transaction is diposed.
            /// </summary>
            public abstract bool IsDisposed { get; }

            /// <summary>
            /// Releases resources owned by this transaction.
            /// </summary>
            public abstract void Dispose();

            /// <summary>
            /// Executes the command in transaction and returns the number of rows affected.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The number of rows affected.</returns>
            protected internal abstract Task<int> ExecuteNonQueryAsync(TCommand command, CancellationToken ct);

            /// <summary>
            /// Performs the operation to execute the command and returns the number of rows affected.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The number of rows affected.</returns>
            protected Task<int> PerformExecuteNonQueryAsync(TCommand command, CancellationToken ct)
            {
                return GetDbSession().InternalExecuteNonQueryAsync(command, ct);
            }

            /// <summary>
            /// Executes the command in transaction and returns a database reader.
            /// </summary>
            /// <param name="model">The model.</param>
            /// <param name="command">The command.</param>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The result reader.</returns>
            protected internal abstract Task<TReader> ExecuteReaderAsync(Model model, TCommand command, CancellationToken ct);

            /// <summary>
            /// Performs the operation to execute the command and returns a database reader.
            /// </summary>
            /// <param name="model">The model.</param>
            /// <param name="command">The command.</param>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The result reader.</returns>
            protected Task<TReader> PerformExecuteReaderAsync(Model model, TCommand command, CancellationToken ct)
            {
                return GetDbSession().InternalExecuteReaderAsync(model, command, ct);
            }

            /// <summary>
            /// Commits the transaction.
            /// </summary>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The async task.</returns>
            public Task CommitAsync(CancellationToken ct)
            {
                return TransactionInvoker.CommitAsync(this, ct);
            }

            /// <summary>
            /// Performs commit transaction operation.
            /// </summary>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The async task.</returns>
            protected internal abstract Task PerformCommitAsync(CancellationToken ct);

            /// <summary>
            /// Rollback the transaction
            /// </summary>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The async task.</returns>
            public Task RollbackAsync(CancellationToken ct)
            {
                return TransactionInvoker.RollbackAsync(this, ct);
            }

            /// <summary>
            /// Performs rollback transaction operation.
            /// </summary>
            /// <param name="ct">The async cancellation token.</param>
            /// <returns>The async task.</returns>
            protected internal abstract Task PerformRollbackAsync(CancellationToken ct);
        }

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <param name="name">The name of the transaction.</param>
        /// <returns>The transaction object.</returns>
        public ITransaction BeginTransaction(string name = null)
        {
            return BeginTransaction(null, name);
        }

        /// <summary>
        /// Begins a transaction at specified isolation level.
        /// </summary>
        /// <param name="isolation">The isolation level.</param>
        /// <param name="name">The name of the transaction.</param>
        /// <returns>The transaction object.</returns>
        public ITransaction BeginTransaction(IsolationLevel? isolation, string name = null)
        {
            return TransactionInvoker.BeginTransaction(this, isolation, name);
        }

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        /// <param name="isolation">The isolation level.</param>
        /// <param name="name">The name of the transaction.</param>
        /// <returns>The transaction object.</returns>
        protected internal abstract ITransaction PerformBeginTransaction(IsolationLevel? isolation, string name);
    }
}
