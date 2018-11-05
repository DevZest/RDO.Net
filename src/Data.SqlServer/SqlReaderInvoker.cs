using System;
using DevZest.Data.Primitives;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data.SqlServer
{
    internal sealed class SqlReaderInvoker : DbReaderInvoker<SqlCommand, SqlReader>
    {
        internal SqlReaderInvoker(SqlSession sqlSession, Model model, SqlCommand sqlCommand)
            : base(sqlSession, model, sqlCommand)
        {
        }

        protected override SqlReader ExecuteCore()
        {
            return SqlReader.Execute(Command, Model);
        }

        protected override Task<SqlReader> ExecuteCoreAsync(CancellationToken cancellationToken)
        {
            return SqlReader.ExecuteAsync(Command, Model, cancellationToken);
        }
    }
}
