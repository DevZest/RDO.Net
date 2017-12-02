using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbNonQueryInterceptor<TCommand> : IResource
        where TCommand : DbCommand
    {
        void Executing(DbNonQueryInvoker<TCommand> invoker);

        void Executed(DbNonQueryInvoker<TCommand> invoker);
    }
}
