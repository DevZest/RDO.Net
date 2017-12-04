using System;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public interface IDbTransactionInterceptor<TConnection, TTransaction> : IExtension
        where TConnection : DbConnection
        where TTransaction : DbTransaction
    {
        void Beginning(DbTransactionInvoker<TConnection, TTransaction> invoker);

        void Began(DbTransactionInvoker<TConnection, TTransaction> invoker);

        void Committing(DbTransactionInvoker<TConnection, TTransaction> invoker);

        void Committed(DbTransactionInvoker<TConnection, TTransaction> invoker);

        void RollingBack(DbTransactionInvoker<TConnection, TTransaction> invoker);

        void RolledBack(DbTransactionInvoker<TConnection, TTransaction> invoker);
    }
}
