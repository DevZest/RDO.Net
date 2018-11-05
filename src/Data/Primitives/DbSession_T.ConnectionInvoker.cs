using DevZest.Data.Addons;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TTransaction, TCommand, TReader>
    {
        private sealed class ConnectionInvoker : AddonInvoker<IDbConnectionInterceptor<TConnection>>
        {
            public ConnectionInvoker(DbSession dbSession, TConnection connection)
                : base(dbSession)
            {
                connection.VerifyNotNull(nameof(connection));
                Connection = connection;
            }

            public TConnection Connection { get; private set; }

            internal Task OpenAsync(CancellationToken cancellationToken)
            {
                return InvokeAsync(PerformOpenAsync(cancellationToken), x => x.OnOpening(Connection, this), x => x.OnOpened(Connection, this));
            }

            private async Task PerformOpenAsync(CancellationToken cancellationToken)
            {
                await Connection.OpenAsync(cancellationToken);
            }

            internal void Close()
            {
                Invoke(() => Connection.Close(), x => x.OnClosing(Connection, this), x => x.OnClosed(Connection, this));
            }
        }
    }
}
