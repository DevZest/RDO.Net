using DevZest.Data.Addons;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TTransaction, TCommand, TReader>
    {
        protected abstract class ReaderInvoker : AddonInvoker<IDbReaderInterceptor<TCommand, TReader>>
        {
            protected ReaderInvoker(DbSession dbSession, Model model, TCommand command)
                : base(dbSession)
            {
                Model = model.VerifyNotNull(nameof(model));
                Command = command.VerifyNotNull(nameof(command));
            }

            public Model Model { get; }

            public TCommand Command { get; }

            public TReader Result { get; private set; }

            protected abstract Task<TReader> ExecuteCoreAsync(CancellationToken cancellationToken);

            internal async Task<TReader> ExecuteAsync(CancellationToken cancellationToken)
            {
                await InvokeAsync(GetAsyncOperation(cancellationToken),
                    x => x.OnExecuting(Model, Command, this),
                    x => x.OnExecuted(Model, Command, Result, this));
                return Result;
            }

            private async Task GetAsyncOperation(CancellationToken cancellationToken)
            {
                Result = await ExecuteCoreAsync(cancellationToken);
            }
        }
    }
}
