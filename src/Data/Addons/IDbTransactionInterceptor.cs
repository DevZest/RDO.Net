using System.Data;
using System.Data.Common;

namespace DevZest.Data.Addons
{
    public interface IDbTransactionInterceptor<TConnection, TTransaction> : IAddon
        where TConnection : DbConnection
        where TTransaction : DbTransaction
    {
        void OnBeginning(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        void OnBegan(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        void OnCommitting(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        void OnCommitted(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        void OnRollingBack(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);

        void OnRolledBack(TConnection connection, IsolationLevel? isolationLevel, TTransaction transaction, AddonInvoker invoker);
    }
}
