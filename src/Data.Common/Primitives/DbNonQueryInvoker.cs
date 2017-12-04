using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public sealed class DbNonQueryInvoker<TCommand> : ExtensibleObjectInvoker<IDbNonQueryInterceptor<TCommand>>
        where TCommand : DbCommand
    {
        public DbNonQueryInvoker(DbSession dbSession, TCommand command)
            : base(dbSession)
        {
            Check.NotNull(command, nameof(command));
            Command = command;
        }

        public TCommand Command { get; private set; }

        public int Result { get; private set; }

        internal int Execute()
        {
            Invoke(() => { Result = Command.ExecuteNonQuery(); }, x => x.Executing(this), x => x.Executed(this));
            return Result;
        }

        internal async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            await InvokeAsync(GetAsyncOperation(cancellationToken), x => x.Executing(this), x => x.Executed(this), cancellationToken);
            return Result;
        }

        private async Task GetAsyncOperation(CancellationToken cancellationToken)
        {
            Result = await Command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
