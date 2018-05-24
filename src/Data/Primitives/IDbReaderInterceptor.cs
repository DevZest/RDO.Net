using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbReaderInterceptor<TCommand, TReader> : IExtension
        where TCommand : DbCommand
        where TReader : DbReader
    {
        void Executing(DbReaderInvoker<TCommand, TReader> invoker);

        void Executed(DbReaderInvoker<TCommand, TReader> invoker);
    }
}
