using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public sealed class DbConnectionInvoker<T> : InterceptableInvoker<IDbConnectionInterceptor<T>>
        where T : DbConnection
    {
        public DbConnectionInvoker(DbSession dbSession, T connection)
            : base(dbSession)
        {
            Check.NotNull(connection, nameof(connection));
            Connection = connection;
        }

        public T Connection { get; private set; }

        internal void Open()
        {
            Invoke(() => Connection.Open(), x => x.Opening(this), x => x.Opened(this));
        }

        internal Task OpenAsync(CancellationToken cancellationToken)
        {
            return InvokeAsync(GetOpenAsyncTask(cancellationToken), x => x.Opening(this), x => x.Opened(this), cancellationToken);
        }

        private async Task GetOpenAsyncTask(CancellationToken cancellationToken)
        {
            await Connection.OpenAsync(cancellationToken);
        }

        internal void Close()
        {
            Invoke(() => Connection.Close(), x => x.Closing(this), x => x.Closed(this));
        }
    }
}
