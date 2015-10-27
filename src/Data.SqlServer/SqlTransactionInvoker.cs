using DevZest.Data.Primitives;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DevZest.Data.SqlServer
{
    internal sealed class SqlTransactionInvoker : DbTransactionInvoker<SqlConnection, SqlTransaction>
    {
        public SqlTransactionInvoker(DbSession dbSession, SqlConnection connection, IsolationLevel isolationLevel)
            : base(dbSession, connection, isolationLevel)
        {
        }

        protected override SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Connection.BeginTransaction(isolationLevel);
        }
    }
}
