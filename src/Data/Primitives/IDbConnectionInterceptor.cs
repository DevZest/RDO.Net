using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbConnectionInterceptor<T> : IAddon
        where T : DbConnection
    {
        void Opening(DbConnectionInvoker<T> invoker);

        void Opened(DbConnectionInvoker<T> invoker);

        void Closing(DbConnectionInvoker<T> invoker);

        void Closed(DbConnectionInvoker<T> invoker);
    }
}
