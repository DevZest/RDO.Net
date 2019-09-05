using DevZest.Data.Addons;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TCommand, TReader>
    {
        /// <summary>
        /// Invokes database recordset reader.
        /// </summary>
        protected abstract class ReaderInvoker : AddonInvoker<IDbReaderInterceptor<TCommand, TReader>>
        {
            /// <summary>
            /// Initializes a new instance of <see cref="ReaderInvoker"/> class.
            /// </summary>
            /// <param name="dbSession">The database session.</param>
            /// <param name="model">The model of the database recordset.</param>
            /// <param name="command">The command to execute.</param>
            protected ReaderInvoker(DbSession dbSession, Model model, TCommand command)
                : base(dbSession)
            {
                Model = model.VerifyNotNull(nameof(model));
                Command = command.VerifyNotNull(nameof(command));
            }

            /// <summary>
            /// Gets the model of the database recordset.
            /// </summary>
            public Model Model { get; }

            /// <summary>
            /// Gets the database command.
            /// </summary>
            public TCommand Command { get; }

            /// <summary>
            /// Gets the reader result.
            /// </summary>
            public TReader Result { get; private set; }

            /// <summary>
            /// Executes the reader.
            /// </summary>
            /// <param name="cancellationToken">The async cancellation token.</param>
            /// <returns>The reader.</returns>
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
