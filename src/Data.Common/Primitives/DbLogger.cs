using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Primitives
{
    public class DbLogger<TConnection, TTransaction, TCommand, TReader> : IDbConnectionInterceptor<TConnection>, IDbTransactionInterceptor<TConnection, TTransaction>, IDbReaderInterceptor<TCommand, TReader>, IDbNonQueryInterceptor<TCommand>
        where TConnection : DbConnection
        where TTransaction : DbTransaction
        where TCommand : DbCommand
        where TReader : DbReader
    {
        internal Action<string> WriteAction { get; set; }

        private LogCategory _logCategory = LogCategory.All;
        internal LogCategory LogCategory
        {
            get { return _logCategory; }
            set { _logCategory = value; }
        }

        /// <summary>Writes the given string to the underlying write delegate.</summary>
        /// <param name="logCategory">The category of the logging output.</param>
        /// <param name="output">The string to write.</param>
        protected virtual void Write(LogCategory logCategory, string output)
        {
            if ((LogCategory & logCategory) == logCategory)
                WriteAction(output);
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();
        /// <summary>
        /// The stop watch used to time executions. This stop watch is started at the end of
        /// <see cref="NonQueryExecuting" /> and <see cref="ReaderExecuting" />
        /// methods and is stopped at the beginning of the <see cref="NonQueryExecuted" />
        /// and <see cref="ReaderExecuted" /> methods.
        /// </summary>
        protected Stopwatch Stopwatch
        {
            get { return _stopwatch; }
        }

        public virtual void ConnectionOpening(DbConnectionInvoker<TConnection> invoker)
        {
        }

        public virtual void ConnectionOpened(DbConnectionInvoker<TConnection> invoker)
        {
            if (invoker.Exception != null)
                Write(LogCategory.ConnectionOpened, invoker.IsAsync ? Strings.DbLogger_ConnectionOpenErrorAsync(DateTimeOffset.Now, invoker.Exception.Message) : Strings.DbLogger_ConnectionOpenError(DateTimeOffset.Now, invoker.Exception.Message));
            else if (invoker.TaskStatus.HasFlag(TaskStatus.Canceled))
                Write(LogCategory.ConnectionOpened, Strings.DbLogger_ConnectionOpenCanceled(DateTimeOffset.Now));
            else
                Write(LogCategory.ConnectionOpened, invoker.IsAsync ? Strings.DbLogger_ConnectionOpenAsync(DateTimeOffset.Now) : Strings.DbLogger_ConnectionOpen(DateTimeOffset.Now));
            Write(LogCategory.ConnectionOpened, Environment.NewLine);
        }

        public virtual void ConnectionClosing(DbConnectionInvoker<TConnection> invoker)
        {
        }

        public virtual void ConnectionClosed(DbConnectionInvoker<TConnection> invoker)
        {
            if (invoker.Exception != null)
                Write(LogCategory.ConnectionClosed, Strings.DbLogger_ConnectionCloseError(DateTimeOffset.Now, invoker.Exception.Message));
            else
                Write(LogCategory.ConnectionClosed, Strings.DbLogger_ConnectionClosed(DateTimeOffset.Now));
            Write(LogCategory.ConnectionClosed, Environment.NewLine);
        }

        public virtual void TransactionBeginning(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
        }

        public virtual void TransactionBegan(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            if (invoker.Exception != null)
                Write(LogCategory.TransactionBegan, Strings.DbLogger_TransactionStartError(DateTimeOffset.Now, invoker.Exception.Message));
            else
                Write(LogCategory.TransactionBegan, Strings.DbLogger_TransactionStarted(DateTimeOffset.Now));
            Write(LogCategory.TransactionBegan, Environment.NewLine);
        }

        public virtual void TransactionCommitting(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
        }

        public virtual void TransactionCommitted(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            if (invoker.Exception != null)
                Write(LogCategory.TransactionCommitted, Strings.DbLogger_TransactionCommitError(DateTimeOffset.Now, invoker.Exception.Message));
            else
                Write(LogCategory.TransactionCommitted, Strings.DbLogger_TransactionCommitted(DateTimeOffset.Now));
            Write(LogCategory.TransactionCommitted, Environment.NewLine);
        }

        public virtual void TransactionRollingBack(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
        }

        public virtual void TransactionRolledBack(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            if (invoker.Exception != null)
                Write(LogCategory.TransactionRolledBack, Strings.DbLogger_TransactionRollbackError(DateTimeOffset.Now, invoker.Exception.Message));
            else
                Write(LogCategory.TransactionRolledBack, Strings.DbLogger_TransactionRolledBack(DateTimeOffset.Now));
            Write(LogCategory.TransactionRolledBack, Environment.NewLine);
        }

        protected virtual void LogCommand(TCommand command)
        {
            Debug.Assert(command != null);

            var commandText = command.CommandText ?? "<null>";
            if (commandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                Write(LogCategory.CommandText, commandText);
            else
            {
                Write(LogCategory.CommandText, commandText);
                Write(LogCategory.CommandText, Environment.NewLine);
            }

            if (command.Parameters != null)
            {
                foreach (var parameter in command.Parameters.OfType<DbParameter>())
                    LogParameter(parameter);
            }
            Write(LogCategory.CommandText, Environment.NewLine);
        }

        private void LogParameter(DbParameter parameter)
        {
            Debug.Assert(parameter != null);

            // -- Name: [Value] (Type = {}, Direction = {}, IsNullable = {}, Size = {}, Precision = {} Scale = {})
            var builder = new StringBuilder();
            builder.Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": '")
                .Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : parameter.Value)
                .Append("' (Type = ")
                .Append(parameter.DbType);

            if (parameter.Direction != ParameterDirection.Input)
                builder.Append(", Direction = ").Append(parameter.Direction);

            if (!parameter.IsNullable)
                builder.Append(", IsNullable = false");

            if (parameter.Size != 0)
                builder.Append(", Size = ").Append(parameter.Size);

            if (parameter.Precision != 0)
                builder.Append(", Precision = ").Append(parameter.Precision);

            if (parameter.Scale != 0)
                builder.Append(", Scale = ").Append(parameter.Scale);

            builder.Append(")").AppendLine();

            Write(LogCategory.CommandText, builder.ToString());
        }

        private void Executing(TCommand command, bool isAsync)
        {
            CommandExecuting(command, isAsync);
            Stopwatch.Restart();
        }

        protected virtual void CommandExecuting(TCommand command, bool isAsync)
        {
            Write(LogCategory.CommandExecuting, Environment.NewLine);
            LogCommand(command);
            Write(LogCategory.CommandExecuting, isAsync ? Strings.DbLogger_CommandExecutingAsync(DateTimeOffset.Now) : Strings.DbLogger_CommandExecuting(DateTimeOffset.Now));
            Write(LogCategory.CommandExecuting, Environment.NewLine);
        }

        private void Executed<TInterceptor, TResult>(TCommand command, InterceptableInvoker<TInterceptor> invoker, TResult result)
            where TInterceptor : class, IInterceptor
        {
            Stopwatch.Stop();
            CommandExecuted(command, invoker, result);
        }

        protected virtual void CommandExecuted<TInterceptor, TResult>(TCommand command, InterceptableInvoker<TInterceptor> invoker, TResult result)
            where TInterceptor : class, IInterceptor
        {
            var exception = invoker.Exception;
            if (exception != null)
                Write(LogCategory.CommandExecuted, Strings.DbLogger_CommandFailed(Stopwatch.ElapsedMilliseconds, exception.Message));
            else if (invoker.TaskStatus.HasFlag(TaskStatus.Canceled))
                Write(LogCategory.CommandExecuted, Strings.DbLogger_CommandCanceled(Stopwatch.ElapsedMilliseconds));
            else
            {
                var resultString = (object)result == null
                    ? "null"
                    : (result is DbReader)
                        ? result.GetType().Name
                        : result.ToString();
                Write(LogCategory.CommandExecuted, Strings.DbLogger_CommandComplete(Stopwatch.ElapsedMilliseconds, resultString));
            }
            Write(LogCategory.CommandExecuted, Environment.NewLine);
            Write(LogCategory.CommandExecuted, Environment.NewLine);
        }

        public void ReaderExecuting(DbReaderInvoker<TCommand, TReader> invoker)
        {
            Executing(invoker.Command, invoker.IsAsync);
        }

        public void ReaderExecuted(DbReaderInvoker<TCommand, TReader> invoker)
        {
            Executed(invoker.Command, invoker, invoker.Result);
        }

        public void NonQueryExecuting(DbNonQueryInvoker<TCommand> invoker)
        {
            Executing(invoker.Command, invoker.IsAsync);
        }

        public void NonQueryExecuted(DbNonQueryInvoker<TCommand> invoker)
        {
            Executed(invoker.Command, invoker, invoker.Result);
        }

        public string FullName
        {
            get { return typeof(DbLogger<TConnection, TTransaction, TCommand, TReader>).FullName; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbReaderInterceptor<TCommand, TReader>.Executing(DbReaderInvoker<TCommand, TReader> invoker)
        {
            ReaderExecuting(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbReaderInterceptor<TCommand, TReader>.Executed(DbReaderInvoker<TCommand, TReader> invoker)
        {
            ReaderExecuted(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbNonQueryInterceptor<TCommand>.Executing(DbNonQueryInvoker<TCommand> invoker)
        {
            NonQueryExecuting(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbNonQueryInterceptor<TCommand>.Executed(DbNonQueryInvoker<TCommand> invoker)
        {
            NonQueryExecuted(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbConnectionInterceptor<TConnection>.Opening(DbConnectionInvoker<TConnection> invoker)
        {
            ConnectionOpening(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbConnectionInterceptor<TConnection>.Opened(DbConnectionInvoker<TConnection> invoker)
        {
            ConnectionOpened(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbConnectionInterceptor<TConnection>.Closing(DbConnectionInvoker<TConnection> invoker)
        {
            ConnectionClosing(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbConnectionInterceptor<TConnection>.Closed(DbConnectionInvoker<TConnection> invoker)
        {
            ConnectionClosed(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.Beginning(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionBeginning(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.Began(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionBegan(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.Committing(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionCommitting(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.Committed(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionCommitted(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.RollingBack(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionRollingBack(invoker);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IDbTransactionInterceptor<TConnection, TTransaction>.RolledBack(DbTransactionInvoker<TConnection, TTransaction> invoker)
        {
            TransactionRolledBack(invoker);
        }
    }
}
