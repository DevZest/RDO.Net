using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbConnectionInterceptor<T> : IAddon
        where T : DbConnection
    {
        void Opening(T connection, AddonInvoker invoker);

        void Opened(T connection, AddonInvoker invoker);

        void Closing(T connection, AddonInvoker invoker);

        void Closed(T connection, AddonInvoker invoker);
    }
}
