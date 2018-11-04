using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbNonQueryInterceptor<TCommand> : IAddon
        where TCommand : DbCommand
    {
        void Executing(DbNonQueryInvoker<TCommand> invoker);

        void Executed(DbNonQueryInvoker<TCommand> invoker);
    }
}
