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

        public sealed override int TransactionCount
        {
            get { return _currentTransaction == null ? 0 : 1; }
        }

        protected sealed override Transaction CurrentTransaction
        {
            get { return _currentTransaction; }
        }

        private sealed class SessionTransaction : Transaction
        {
            public static SessionTransaction Create(MySqlSession mySqlSession)
            {
                if (mySqlSession.TransactionCount == 0)
                    return new SessionTransaction(mySqlSession, mySqlSession.Connection.BeginTransaction());
                else
                    throw new InvalidOperationException();
            }

            public static SessionTransaction Create(MySqlSession sqlSession, IsolationLevel isolation)
            {
                if (sqlSession.TransactionCount == 0)
                    return new SessionTransaction(sqlSession, sqlSession.Connection.BeginTransaction(isolation));
                else
                    throw new InvalidOperationException();
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

            public sealed override void Dispose()
            {
                if (_isDisposed)
                    return;

                PerformDispose();
                CurrentTransaction = null;
                _isFrozen = true;
                _isDisposed = true;
            }

            public sealed override Task CommitAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                VerifyNotFrozen();
                MySqlTransaction.Commit();
                _isFrozen = true;
                return Task.CompletedTask;
            }

            public sealed override Task RollbackAsync(CancellationToken ct)
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
                    throw new InvalidOperationException();
            }

            private void PerformDispose()
            {
                if (_isFrozen)
                    return;

                MySqlTransaction.Rollback();
            }
        }

        public sealed override ITransaction BeginTransaction(string name = null)
        {
            return SessionTransaction.Create(this);
        }

        public sealed override ITransaction BeginTransaction(IsolationLevel isolation, string name = null)
        {
            return SessionTransaction.Create(this, isolation);
        }
    }
}
