using DevZest.Data.Addons;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TTransaction, TCommand, TReader>
    {
        private sealed class NonQueryCommandInvoker : AddonInvoker<IDbNonQueryInterceptor<TCommand>>
        {
            public NonQueryCommandInvoker(DbSession dbSession, TCommand command)
                : base(dbSession)
            {
                command.VerifyNotNull(nameof(command));
                Command = command;
            }

            public TCommand Command { get; private set; }

            public int Result { get; private set; }

            internal int Execute()
            {
                Invoke(() => { Result = Command.ExecuteNonQuery(); }, x => x.OnExecuting(Command, this), x => x.OnExecuted(Command, Result, this));
                return Result;
            }

            internal async Task<int> ExecuteAsync(CancellationToken cancellationToken)
            {
                await InvokeAsync(GetAsyncOperation(cancellationToken), x => x.OnExecuting(Command, this), x => x.OnExecuted(Command, Result, this));
                return Result;
            }

            private async Task GetAsyncOperation(CancellationToken cancellationToken)
            {
                Result = await Command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }
}
