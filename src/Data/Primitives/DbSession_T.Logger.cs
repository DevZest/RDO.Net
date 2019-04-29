﻿using System;
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
                if ((LogCategory & logCategory) == logCategory)
                    WriteAction(output);
            }

            private readonly Stopwatch _stopwatch = new Stopwatch();
            /// <summary>
            /// The stop watch used to time executions. This stop watch is started at the end of
            /// <see cref="OnCommandExecuting" /> and is stopped at the beginning of the <see cref="OnCommandExecuted" />.
            /// </summary>
            protected Stopwatch Stopwatch
            {
                get { return _stopwatch; }
            }

            public virtual void OnConnectionOpening(TConnection connection, AddonInvoker invoker)
            {
            }

            public virtual void OnConnectionOpened(TConnection connection, AddonInvoker invoker)
            {
                if (invoker.Exception != null)
                    Write(LogCategory.ConnectionOpened, invoker.IsAsync ? DiagnosticMessages.DbLogger_ConnectionOpenErrorAsync(DateTimeOffset.Now, invoker.Exception.Message) : DiagnosticMessages.DbLogger_ConnectionOpenError(DateTimeOffset.Now, invoker.Exception.Message));
                else if (invoker.TaskStatus.HasFlag(TaskStatus.Canceled))
                    Write(LogCategory.ConnectionOpened, DiagnosticMessages.DbLogger_ConnectionOpenCanceled(DateTimeOffset.Now));
                else
                    Write(LogCategory.ConnectionOpened, invoker.IsAsync ? DiagnosticMessages.DbLogger_ConnectionOpenAsync(DateTimeOffset.Now) : DiagnosticMessages.DbLogger_ConnectionOpen(DateTimeOffset.Now));
                Write(LogCategory.ConnectionOpened, Environment.NewLine);
            }

            public virtual void OnConnectionClosing(TConnection connection, AddonInvoker invoker)
            {
            }

            public virtual void OnConnectionClosed(TConnection connection, AddonInvoker invoker)
            {
                if (invoker.Exception != null)
                    Write(LogCategory.ConnectionClosed, DiagnosticMessages.DbLogger_ConnectionCloseError(DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.ConnectionClosed, DiagnosticMessages.DbLogger_ConnectionClosed(DateTimeOffset.Now));
                Write(LogCategory.ConnectionClosed, Environment.NewLine);
            }

            public virtual void OnTransactionBeginning(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
            }

            public virtual void OnTransactionBegan(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionBegan, DiagnosticMessages.DbLogger_TransactionStartError(isolationLevel, name, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionBegan, DiagnosticMessages.DbLogger_TransactionStarted(isolationLevel, name, DateTimeOffset.Now));
                Write(LogCategory.TransactionBegan, Environment.NewLine);
            }

            public virtual void OnTransactionCommitting(ITransaction transaction, AddonInvoker invoker)
            {
            }

            public virtual void OnTransactionCommitted(ITransaction transaction, AddonInvoker invoker)
            {
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionCommitted, DiagnosticMessages.DbLogger_TransactionCommitError(transaction.Name, transaction.Level, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionCommitted, DiagnosticMessages.DbLogger_TransactionCommitted(transaction.Name, transaction.Level, DateTimeOffset.Now));
                Write(LogCategory.TransactionCommitted, Environment.NewLine);
            }

            public virtual void OnTransactionRollingBack(ITransaction transaction, AddonInvoker invoker)
            {
            }

            public virtual void OnTransactionRolledBack(ITransaction transaction, AddonInvoker invoker)
            {
                if (invoker.Exception != null)
                    Write(LogCategory.TransactionRolledBack, DiagnosticMessages.DbLogger_TransactionRollbackError(transaction.Name, transaction.Level, DateTimeOffset.Now, invoker.Exception.Message));
                else
                    Write(LogCategory.TransactionRolledBack, DiagnosticMessages.DbLogger_TransactionRolledBack(transaction.Name, transaction.Level, DateTimeOffset.Now));
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

            private void OnExecuting(TCommand command, AddonInvoker invoker)
            {
                OnCommandExecuting(command, invoker);
                Stopwatch.Restart();
            }

            protected virtual void OnCommandExecuting(TCommand command, AddonInvoker invoker)
            {
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

            protected virtual void OnCommandExecuted<TResult>(TCommand command, TResult result, AddonInvoker invoker)
            {
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
                OnConnectionOpening(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnOpened(TConnection connection, AddonInvoker invoker)
            {
                OnConnectionOpened(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnClosing(TConnection connection, AddonInvoker invoker)
            {
                OnConnectionClosing(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbConnectionInterceptor<TConnection>.OnClosed(TConnection connection, AddonInvoker invoker)
            {
                OnConnectionClosed(connection, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnBeginning(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                OnTransactionBeginning(isolationLevel, name, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnBegan(IsolationLevel? isolationLevel, string name, AddonInvoker invoker)
            {
                OnTransactionBegan(isolationLevel, name, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnCommitting(ITransaction transaction, AddonInvoker invoker)
            {
                OnTransactionCommitting(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnCommitted(ITransaction transaction, AddonInvoker invoker)
            {
                OnTransactionCommitted(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnRollingBack(ITransaction transaction, AddonInvoker invoker)
            {
                OnTransactionRollingBack(transaction, invoker);
            }

            [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
            void IDbTransactionInterceptor.OnRolledBack(ITransaction transaction, AddonInvoker invoker)
            {
                OnTransactionRolledBack(transaction, invoker);
            }
        }
    }
}
