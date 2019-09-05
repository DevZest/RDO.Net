using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using DevZest.Data.Addons;

namespace DevZest.Data.Primitives
{
    partial class DbSession<TConnection, TCommand, TReader>
    {
        /// <summary>
        /// Logs records to a variety of destinations such as log files or the console.
        /// </summary>
        protected class Logger :
            IDbConnectionInterceptor<TConnection>,
            IDbTransactionInterceptor,
            IDbReaderInterceptor<TCommand, TReader>,
            IDbNonQueryInterceptor<TCommand>
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
                Debug.Assert(ShouldLog(logCategory));
                WriteAction(output);
            }

            private bool ShouldLog(LogCategory logCategory)
            {
                return (LogCategory & logCategory) == logCategory;
            }

            private readonly Stopwatch _stopwatch = new Stopwatch();
            /// <summary>
            /// Gets the stop watch used to record time elapsed during executions.
            /// </summary>
            protected Stopwatch Stopwatch
            {
                get { return _stopwatch; }
            }

            /// <summary>
            /// Writes logging message for connection opening.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="inovker">The invoker.</param>
            /// <remarks>The default implementation does nothing.</remarks>
            protected virtual void WriteConnectionOpening(TConnection connection, AddonInvoker inovker)
            {
                Debug.Assert(ShouldLog(LogCategory.ConnectionOpening));
            }

            /// <summary>
            /// Writes logging message for connection opened.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteConnectionOpened(TConnection connection, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.ConnectionOpened));
                if (invoker.Exception != null)
                    Write(LogCategory.ConnectionOpened, invoker.IsAsync ? DiagnosticMessages.DbLogger_ConnectionOpenErrorAsync(DateTimeOffset.Now, invoker.Exception.Message) : DiagnosticMessages.DbLogger_ConnectionOpenError(DateTimeOffset.Now, invoker.Exception.Message));
                else if (invoker.TaskStatus.HasFlag(TaskStatus.Canceled))
                    Write(LogCategory.ConnectionOpened, DiagnosticMessages.DbLogger_ConnectionOpenCanceled(DateTimeOffset.Now));
                else
                    Write(LogCategory.ConnectionOpened, invoker.IsAsync ? DiagnosticMessages.DbLogger_ConnectionOpenAsync(DateTimeOffset.Now) : DiagnosticMessages.DbLogger_ConnectionOpen(DateTimeOffset.Now));
                Write(LogCategory.ConnectionOpened, Environment.NewLine);
            }

            /// <summary>
            /// Writes logging message for connection closing.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="invoker">The invoker.</param>
            /// <remarks>The default implementation does nothing.</remarks>
            protected virtual void WriteConnectionClosing(TConnection connection, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.ConnectionClosing));
            }

            /// <summary>
            /// Writes logging message for connection closed.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteConnectionClosed(TConnection connection, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.ConnectionClosed));
                if (invoker.Exception != null)
                    Write(LogCategory.ConnectionClosed, DiagnosticMessages.DbLogger_ConnectionCloseError(DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.ConnectionClosed, DiagnosticMessages.DbLogger_ConnectionClosed(DateTimeOffset.Now));
                Write(LogCategory.ConnectionClosed, Environment.NewLine);
            }

            /// <summary>
            /// Writes logging message for transaction beginning.
            /// </summary>
            /// <param name="isolationLevel">The transaction isolation level.</param>
            /// <param name="name">The name of the transaction.</param>
            /// <param name="invoker">The invoker.</param>
            /// <remarks>The default implementation does nothing.</remarks>
            protected virtual void WriteTransactionBeginning(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionBeginning));
            }

            /// <summary>
            /// Writes logging message for transaction began.
            /// </summary>
            /// <param name="isolationLevel">The transaction isolation level.</param>
            /// <param name="name">The name of the transaction.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteTransactionBegan(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionBegan));
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionBegan, DiagnosticMessages.DbLogger_TransactionStartError(isolationLevel, name, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionBegan, DiagnosticMessages.DbLogger_TransactionStarted(isolationLevel, name, DateTimeOffset.Now));
                Write(LogCategory.TransactionBegan, Environment.NewLine);
            }

            /// <summary>
            /// Writes logging message for transaction committing.
            /// </summary>
            /// <param name="transaction">The transation.</param>
            /// <param name="invoker">The invoker.</param>
            /// <remarks>The default implementation does nothing.</remarks>
            protected virtual void WriteTransactionCommitting(ITransaction transaction, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionCommitting));
            }

            /// <summary>
            /// Writes logging message for transaction committed.
            /// </summary>
            /// <param name="transaction">The transation.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteTransactionCommitted(ITransaction transaction, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionCommitted));
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionCommitted, DiagnosticMessages.DbLogger_TransactionCommitError(transaction.Name, transaction.Level, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionCommitted, DiagnosticMessages.DbLogger_TransactionCommitted(transaction.Name, transaction.Level, DateTimeOffset.Now));
                Write(LogCategory.TransactionCommitted, Environment.NewLine);
            }

            /// <summary>
            /// Writes logging message for transaction rolling back.
            /// </summary>
            /// <param name="transaction">The transation.</param>
            /// <param name="invoker">The invoker.</param>
            /// <remarks>The default implementation does nothing.</remarks>
            protected virtual void WriteTransactionRollingBack(ITransaction transaction, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionRollingBack));
            }

            /// <summary>
            /// Writes logging message for transaction rolled back.
            /// </summary>
            /// <param name="transaction">The transation.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteTransactionRolledBack(ITransaction transaction, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.TransactionRolledBack));
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionRolledBack, DiagnosticMessages.DbLogger_TransactionRollbackError(transaction.Name, transaction.Level, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionRolledBack, DiagnosticMessages.DbLogger_TransactionRolledBack(transaction.Name, transaction.Level, DateTimeOffset.Now));
                Write(LogCategory.TransactionRolledBack, Environment.NewLine);
            }

            private void LogCommand(TCommand command)
            {
                if (ShouldLog(LogCategory.CommandText))
                    WriteCommand(command);
            }

            /// <summary>
            /// Writes command into logging.
            /// </summary>
            /// <param name="command">The command.</param>
            protected virtual void WriteCommand(TCommand command)
            {
                Debug.Assert(ShouldLog(LogCategory.CommandText));
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
                        WriteParameter(parameter);
                }
                Write(LogCategory.CommandText, Environment.NewLine);
            }

            private void WriteParameter(DbParameter parameter)
            {
                Debug.Assert(ShouldLog(LogCategory.CommandText));
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

            private void OnExecuting(TCommand command, AddonInvoker invoker)
            {
                LogCommandExecuting(command, invoker);
                Stopwatch.Restart();
            }

            private void LogCommandExecuting(TCommand command, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.CommandExecuting))
                    WriteCommandExecuting(command, invoker);
                else
                    LogCommand(command);
            }

            /// <summary>
            /// Writes logging message for command executing.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteCommandExecuting(TCommand command, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.CommandExecuting));
                Write(LogCategory.CommandExecuting, Environment.NewLine);
                LogCommand(command);
                Write(LogCategory.CommandExecuting, invoker.IsAsync ? DiagnosticMessages.DbLogger_CommandExecutingAsync(DateTimeOffset.Now) : DiagnosticMessages.DbLogger_CommandExecuting(DateTimeOffset.Now));
                Write(LogCategory.CommandExecuting, Environment.NewLine);
            }

            private void OnExecuted<TResult>(TCommand command, TResult result, AddonInvoker invoker)
            {
                Stopwatch.Stop();
                OnCommandExecuted(command, result, invoker);
            }

            private void OnCommandExecuted<TResult>(TCommand command, TResult result, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.CommandExecuted))
                    WriteCommandExecuted(command, result, invoker);
            }

            /// <summary>
            /// Writes logging message for command executed.
            /// </summary>
            /// <typeparam name="TResult">Type of the command result.</typeparam>
            /// <param name="command">The command.</param>
            /// <param name="result">The command result.</param>
            /// <param name="invoker">The invoker.</param>
            protected virtual void WriteCommandExecuted<TResult>(TCommand command, TResult result, AddonInvoker invoker)
            {
                Debug.Assert(ShouldLog(LogCategory.CommandExecuted));
                var exception = invoker.Exception;
                if (exception != null)
                    Write(LogCategory.CommandExecuted, DiagnosticMessages.DbLogger_CommandFailed(Stopwatch.ElapsedMilliseconds, exception.Message));
                else if (invoker.TaskStatus.HasFlag(TaskStatus.Canceled))
                    Write(LogCategory.CommandExecuted, DiagnosticMessages.DbLogger_CommandCanceled(Stopwatch.ElapsedMilliseconds));
                else
                {
                    var resultString = (object)result == null
                        ? "null"
                        : (result is DbReader)
                            ? result.GetType().Name
                            : result.ToString();
                    Write(LogCategory.CommandExecuted, DiagnosticMessages.DbLogger_CommandComplete(Stopwatch.ElapsedMilliseconds, resultString));
                }
                Write(LogCategory.CommandExecuted, Environment.NewLine);
                Write(LogCategory.CommandExecuted, Environment.NewLine);
            }

            object IAddon.Key
            {
                get { return typeof(Logger); }
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbReaderInterceptor<TCommand, TReader>.OnExecuting(Model model, TCommand command, AddonInvoker invoker)
            {
                OnExecuting(command, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbReaderInterceptor<TCommand, TReader>.OnExecuted(Model model, TCommand command, TReader result, AddonInvoker invoker)
            {
                OnExecuted(command, result, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbNonQueryInterceptor<TCommand>.OnExecuting(TCommand command, AddonInvoker invoker)
            {
                OnExecuting(command, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbNonQueryInterceptor<TCommand>.OnExecuted(TCommand command, int result, AddonInvoker invoker)
            {
                OnExecuted(command, result, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnOpening(TConnection connection, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.ConnectionOpening))
                    WriteConnectionOpening(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnOpened(TConnection connection, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.ConnectionOpened))
                    WriteConnectionOpened(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnClosing(TConnection connection, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.ConnectionClosing))
                    WriteConnectionClosing(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnClosed(TConnection connection, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.ConnectionClosed))
                    WriteConnectionClosed(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnBeginning(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionBeginning))
                    WriteTransactionBeginning(isolationLevel, name, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnBegan(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionBegan))
                    WriteTransactionBegan(isolationLevel, name, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnCommitting(ITransaction transaction, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionCommitting))
                    WriteTransactionCommitting(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnCommitted(ITransaction transaction, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionCommitted))
                    WriteTransactionCommitted(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnRollingBack(ITransaction transaction, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionRollingBack))
                    WriteTransactionRollingBack(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnRolledBack(ITransaction transaction, AddonInvoker invoker)
            {
                if (ShouldLog(LogCategory.TransactionRolledBack))
                    WriteTransactionRolledBack(transaction, invoker);
            }
        }
    }
}
