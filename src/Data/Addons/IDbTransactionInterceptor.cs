using System.Data;
using System.Data.Common;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents <see cref="Model"/> addon to intercept <see cref="DbTransaction"/> execution.
    /// </summary>
    public interface IDbTransactionInterceptor : IAddon
    {
        /// <summary>
        /// Intercepts before <see cref="ITransaction"/> beginning.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="name">The name of the transaction.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnBeginning(IsolationLevel? isolationLevel, string name, AddonInvoker invoker);

        /// <summary>
        /// Intercepts after <see cref="ITransaction"/> began.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/>.</param>
        /// <param name="name">The name of the transaction.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnBegan(IsolationLevel? isolationLevel, string name, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="ITransaction"/> committing.
        /// </summary>
        /// <param name="transaction">The <see cref="ITransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnCommitting(ITransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="ITransaction"/> committed.
        /// </summary>
        /// <param name="transaction">The <see cref="ITransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnCommitted(ITransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="ITransaction"/> rolling back.
        /// </summary>
        /// <param name="transaction">The <see cref="ITransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnRollingBack(ITransaction transaction, AddonInvoker invoker);

        /// <summary>
        /// Intercepts before <see cref="ITransaction"/> rolled back.
        /// </summary>
        /// <param name="transaction">The <see cref="ITransaction"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="ITransaction"/> execution.</param>
        void OnRolledBack(ITransaction transaction, AddonInvoker invoker);
    }
}
