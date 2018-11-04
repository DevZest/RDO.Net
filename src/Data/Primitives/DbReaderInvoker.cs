using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class DbReaderInvoker<TCommand, TReader> : AddonInvoker<IDbReaderInterceptor<TCommand, TReader>>
        where TCommand : DbCommand
        where TReader : DbReader
    {
        protected DbReaderInvoker(DbSession dbSession, TCommand command, Model model)
            : base(dbSession)
        {
            command.VerifyNotNull(nameof(command));
            model.VerifyNotNull(nameof(model));
            Command = command;
            Model = model;
        }

        public TCommand Command { get; private set; }

        public Model Model { get; private set; }

        public TReader Result { get; private set; }

        protected abstract TReader ExecuteCore();

        protected abstract Task<TReader> ExecuteCoreAsync(CancellationToken cancellationToken);

        internal TReader Execute()
        {
            base.Invoke(() => { Result = ExecuteCore(); },
                x => x.Executing(this),
                x => x.Executed(this));
            return Result;
        }

        internal async Task<TReader> ExecuteAsync(CancellationToken cancellationToken)
        {
            await InvokeAsync(GetAsyncOperation(cancellationToken),
                x => x.Executing(this),
                x => x.Executed(this));
            return Result;
        }

        private async Task GetAsyncOperation(CancellationToken cancellationToken)
        {
            Result = await ExecuteCoreAsync(cancellationToken);
        }
    }
}
