using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class DbReaderInvoker<TCommand, TReader> : AddonInvoker<IDbReaderInterceptor<TCommand, TReader>>
        where TCommand : DbCommand
        where TReader : DbReader
    {
        protected DbReaderInvoker(DbSession dbSession, Model model, TCommand command)
            : base(dbSession)
        {
            Model = model.VerifyNotNull(nameof(model));
            Command = command.VerifyNotNull(nameof(command));
        }

        public Model Model { get; }

        public TCommand Command { get; }

        public TReader Result { get; private set; }

        protected abstract TReader ExecuteCore();

        protected abstract Task<TReader> ExecuteCoreAsync(CancellationToken cancellationToken);

        internal TReader Execute()
        {
            base.Invoke(() => { Result = ExecuteCore(); },
                x => x.Executing(Model, Command, this),
                x => x.Executed(Model, Command, Result, this));
            return Result;
        }

        internal async Task<TReader> ExecuteAsync(CancellationToken cancellationToken)
        {
            await InvokeAsync(GetAsyncOperation(cancellationToken),
                x => x.Executing(Model, Command, this),
                x => x.Executed(Model, Command, Result, this));
            return Result;
        }

        private async Task GetAsyncOperation(CancellationToken cancellationToken)
        {
            Result = await ExecuteCoreAsync(cancellationToken);
        }
    }
}
