using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TTransaction, TCommand, TReader>
    {
        private sealed class ConnectionInterceptorInvoker : AddonInvoker<IDbConnectionInterceptor<TConnection>>
        {
            public ConnectionInterceptorInvoker(DbSession dbSession, TConnection connection)
                : base(dbSession)
            {
                connection.VerifyNotNull(nameof(connection));
                Connection = connection;
            }

            public TConnection Connection { get; private set; }

            internal Task OpenAsync(CancellationToken cancellationToken)
            {
                return InvokeAsync(PerformOpenAsync(cancellationToken), x => x.Opening(Connection, this), x => x.Opened(Connection, this));
            }

            private async Task PerformOpenAsync(CancellationToken cancellationToken)
            {
                await Connection.OpenAsync(cancellationToken);
            }

            internal void Close()
            {
                Invoke(() => Connection.Close(), x => x.Closing(Connection, this), x => x.Closed(Connection, this));
            }
        }
    }
}
