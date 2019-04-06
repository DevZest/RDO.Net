using System.Data;
using System.Data.Common;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents <see cref="Model"/> addon to intercept <see cref="DbTransaction"/> execution.
    /// </summary>
    /// <typeparam name="TConnection">The concrete type of <see cref="DbConnection"/>.</typeparam>
    /// <typeparam name="TTransaction">The concrete type of <see cref="DbTransaction"/>.</typeparam>
    public interface IDbTransactionInterceptor<TConnection, TTransaction> : IAddon
        where TConnection : DbConnection
        where TTransaction : DbTransaction
    {
        /// <summary>
        /// Intercepts before <see cref="DbTransaction"/> beginning.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnBeginning(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts after <see cref="DbTransaction"/> began.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnBegan(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="DbTransaction"/> committing.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnCommitting(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="DbTransaction"/> committed.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnCommitted(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="DbTransaction"/> rolling back.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnRollingBack(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="DbTransaction"/> rolled back.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbTransaction"/> execution.</param>
        void OnRolledBack(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);
    }
}
