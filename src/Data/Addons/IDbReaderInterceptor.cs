using DevZest.Data.Primitives;
using System.Data.Common;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents <see cref="Model"/> addon to intercept <see cref="DbReader"/> execution.
    /// </summary>
    /// <typeparam name="TCommand">Concrete type of <see cref="DbCommand"/>.</typeparam>
    /// <typeparam name="TReader">Concrete type of <see cref="DbReader"/>.</typeparam>
    public interface IDbReaderInterceptor<TCommand, TReader> : IAddon
        where TCommand : DbCommand
        where TReader : DbReader
    {
        /// <summary>
        /// Intercepts before <see cref="DbReader"/> execution.
        /// </summary>
        /// <param name="model">The <see cref="Model"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> which opens the <see cref="DbReader"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbCommand"/> execution.</param>
        void OnExecuting(Model model, TCommand command, AddonInvoker invoker);

        /// <summary>
        /// Intercepts after <see cref="DbReader"/> execution.
        /// </summary>
        /// <param name="model">The <see cref="Model"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> which opens the <see cref="DbReader"/>.</param>
        /// <param name="reader">The <see cref="DbReader"/> opened by <paramref name="command"/>.</param>
        /// <param name="invoker">The <see cref="AddonInvoker"/> which wraps the <see cref="DbReader"/> execution.</param>
        void OnExecuted(Model model, TCommand command, TReader reader, AddonInvoker invoker);
    }
}
