using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data.SqlServer
{
    partial class SqlSession
    {
        private sealed class SqlReaderInvoker : ReaderInvoker
        {
            internal SqlReaderInvoker(SqlSession sqlSession, Model model, SqlCommand sqlCommand)
                : base(sqlSession, model, sqlCommand)
            {
            }

            protected override Task<SqlReader> ExecuteCoreAsync(CancellationToken cancellationToken)
            {
                return SqlReader.ExecuteAsync(Command, Model, cancellationToken);
            }
        }
    }
}
