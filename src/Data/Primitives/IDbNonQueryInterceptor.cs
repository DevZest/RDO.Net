using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbNonQueryInterceptor<TCommand> : IAddon
        where TCommand : DbCommand
    {
        void Executing(TCommand command, int result, AddonInvoker invoker);

        void Executed(TCommand command, int result, AddonInvoker invoker);
    }
}
