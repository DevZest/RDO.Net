using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbConnectionInterceptor<T> : IAddon
        where T : DbConnection
    {
        void OnOpening(T connection, AddonInvoker invoker);

        void OnOpened(T connection, AddonInvoker invoker);

        void OnClosing(T connection, AddonInvoker invoker);

        void OnClosed(T connection, AddonInvoker invoker);
    }
}
