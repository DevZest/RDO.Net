using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbReaderInterceptor<TCommand, TReader> : IAddon
        where TCommand : DbCommand
        where TReader : DbReader
    {
        void Executing(Model model, TCommand command, AddonInvoker invoker);

        void Executed(Model model, TCommand command, TReader result, AddonInvoker invoker);
    }
}
