using System;
using DevZest.Data.Primitives;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data.SqlServer
{
    internal sealed class SqlReaderInvoker : DbReaderInvoker<SqlCommand, SqlReader>
    {
        internal SqlReaderInvoker(SqlSession sqlSession, SqlCommand sqlCommand, Model model)
            : base(sqlSession, sqlCommand, model)
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
