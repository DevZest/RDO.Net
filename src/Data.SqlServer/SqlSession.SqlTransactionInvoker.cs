using DevZest.Data.Primitives;
using System.Data;
using System.Data.SqlClient;

namespace DevZest.Data.SqlServer
{
    partial class SqlSession
    {
        private sealed class SqlTransactionInterceptorInvoker : TransactionInvoker
        {
            public SqlTransactionInterceptorInvoker(DbSession dbSession, SqlConnection connection, IsolationLevel? isolationLevel)
                : base(dbSession, connection, isolationLevel)
            {
            }

            protected override SqlTransaction BeginTransaction()
            {
                if (IsolationLevel.HasValue)
                    return Connection.BeginTransaction(IsolationLevel.Value);
                else
                    return Connection.BeginTransaction();
            }
        }
    }
}
