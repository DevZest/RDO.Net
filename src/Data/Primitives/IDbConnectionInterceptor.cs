using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbConnectionInterceptor<T> : IAddon
        where T : DbConnection
    {
        void Opening(DbConnectionInterceptorInvoker<T> invoker);

        void Opened(DbConnectionInterceptorInvoker<T> invoker);

        void Closing(DbConnectionInterceptorInvoker<T> invoker);

        void Closed(DbConnectionInterceptorInvoker<T> invoker);
    }
}
