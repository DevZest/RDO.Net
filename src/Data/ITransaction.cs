using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a database transaction.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Gets the name of the transaction.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the level of nested transactions.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The async task.</returns>
        Task CommitAsync(CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Rollback the transaction.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The async task.</returns>
        Task RollbackAsync(CancellationToken ct = default(CancellationToken));
    }
}
