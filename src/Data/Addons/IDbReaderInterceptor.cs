using DevZest.Data.Primitives;
using System.Data.Common;

namespace DevZest.Data.Addons
{
    public interface IDbReaderInterceptor<TCommand, TReader> : IAddon
        where TCommand : DbCommand
        where TReader : DbReader
    {
        void OnExecuting(Model model, TCommand command, AddonInvoker invoker);

        void OnExecuted(Model model, TCommand command, TReader result, AddonInvoker invoker);
    }
}
