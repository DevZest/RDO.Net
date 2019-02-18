using System.Threading.Tasks;
using System.Threading;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    partial class MySqlSession
    {
        private sealed class MySqlReaderInvoker : ReaderInvoker
        {
            internal MySqlReaderInvoker(MySqlSession mySqlSession, Model model, MySqlCommand mySqlCommand)
                : base(mySqlSession, model, mySqlCommand)
            {
            }

            protected override Task<MySqlReader> ExecuteCoreAsync(CancellationToken cancellationToken)
            {
                return MySqlReader.ExecuteAsync(Command, Model, cancellationToken);
            }
        }
    }
}
