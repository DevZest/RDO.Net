using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System.Data;

namespace DevZest.Data.MySql
{
    partial class MySqlSession
    {
        private sealed class MySqlTransactionInterceptorInvoker : TransactionInvoker
        {
            public MySqlTransactionInterceptorInvoker(DbSession dbSession, MySqlConnection connection, IsolationLevel? isolationLevel)
                : base(dbSession, connection, isolationLevel)
            {
            }

            protected override MySqlTransaction BeginTransaction()
            {
                if (IsolationLevel.HasValue)
                    return Connection.BeginTransaction(IsolationLevel.Value);
                else
                    return Connection.BeginTransaction();
            }
        }
    }
}
