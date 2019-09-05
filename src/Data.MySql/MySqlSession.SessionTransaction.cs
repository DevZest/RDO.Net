using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    partial class MySqlSession
    {
        private SessionTransaction _currentTransaction;

        /// <inheritdoc/>
        public sealed override int TransactionCount
        {
            get { return _currentTransaction == null ? 0 : 1; }
        }

        /// <inheritdoc/>
        protected sealed override Transaction CurrentTransaction
        {
            get { return _currentTransaction; }
        }

        private sealed class SessionTransaction : Transaction
        {
            public static SessionTransaction Create(MySqlSession mySqlSession, IsolationLevel? isolation)
            {
                if (mySqlSession.TransactionCount == 0)
                {
                    var connection = mySqlSession.Connection;
                    var transaction = isolation.HasValue ? connection.BeginTransaction(isolation.Value) : connection.BeginTransaction();
                    return new SessionTransaction(mySqlSession, transaction);
                }
                else
                    throw new NotSupportedException(DiagnosticMessages.NestedTransactionNotSupported);
            }

            public SessionTransaction(MySqlSession mySqlSession, MySqlTransaction mySqlTransaction)
            {
                MySqlSession = mySqlSession;
                MySqlTransaction = mySqlTransaction;
                Debug.Assert(CurrentTransaction == null);
                CurrentTransaction = this;
            }

            public MySqlTransaction MySqlTransaction { get; }

            private bool IsCurrent
            {
                get { return MySqlSession.CurrentTransaction == this; }
            }

            private void VerifyIsCurrent()
            {
                if (!IsCurrent)
                    throw new InvalidOperationException(DiagnosticMessages.VerifyIsCurrentTransaction);
            }

            private MySqlSession MySqlSession { get; }

            protected sealed override DbSession<MySqlConnection, MySqlCommand, MySqlReader> GetDbSession()
            {
                return MySqlSession;
            }

            private SessionTransaction CurrentTransaction
            {
                get { return MySqlSession._currentTransaction; }
                set { MySqlSession._currentTransaction = value; }
            }

            private bool _isDisposed;
            public sealed override bool IsDisposed
            {
                get { return _isDisposed; }
            }

            public override string Name => null;

            public override int Level => 0;

            public sealed override void Dispose()
            {
                if (_isDisposed)
                    return;

                PerformDispose();
                CurrentTransaction = null;
                _isDisposed = true;
            }

            protected sealed override Task PerformCommitAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                VerifyNotFrozen();
                MySqlTransaction.Commit();
                _isFrozen = true;
                return Task.CompletedTask;
            }

            protected sealed override Task PerformRollbackAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                VerifyNotFrozen();
                MySqlTransaction.Rollback();
                _isFrozen = true;
                return Task.CompletedTask;
            }

            protected override Task<int> ExecuteNonQueryAsync(MySqlCommand command, CancellationToken ct)
            {
                VerifyNotFrozen();
                command.Transaction = MySqlTransaction;
                return PerformExecuteNonQueryAsync(command, ct);
            }

            protected override Task<MySqlReader> ExecuteReaderAsync(Model model, MySqlCommand command, CancellationToken ct)
            {
                VerifyNotFrozen();
                command.Transaction = MySqlTransaction;
                return PerformExecuteReaderAsync(model, command, ct);
            }

            private bool _isFrozen;

            private void VerifyNotFrozen()
            {
                if (_isFrozen)
                    throw new InvalidOperationException(DiagnosticMessages.VerifyNotFrozenTransaction);
            }

            private void PerformDispose()
            {
                if (_isFrozen)
                    return;

                MySqlTransaction.Rollback();
            }
        }

        /// <inheritdoc/>
        protected sealed override ITransaction PerformBeginTransaction(IsolationLevel? isolation, string name = null)
        {
            return SessionTransaction.Create(this, isolation);
        }
    }
}
