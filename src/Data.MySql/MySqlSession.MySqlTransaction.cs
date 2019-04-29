using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    partial class MySqlSession
    {
        private readonly Stack<MySqlTransaction> _transactions = new Stack<MySqlTransaction>();

        public sealed override int TransactionCount
        {
            get { return _transactions.Count; }
        }

        protected sealed override Transaction CurrentTransaction
        {
            get { return _transactions.Count == 0 ? null : _transactions.Peek(); }
        }

        private sealed class MySqlTransaction : Transaction
        {
            public MySqlTransaction(MySqlSession mySqlSession, string name)
            {
                MySqlSession = mySqlSession;
                Level = mySqlSession.TransactionCount;
                Name = GetName(name);
                Transactions.Push(this);
            }

            private string GetName(string name)
            {
                if (string.IsNullOrEmpty(name))
                    name = "_SYS_XACT";
                return string.Format("{0}_{1}", name, Level);
            }

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

            private Stack<MySqlTransaction> Transactions
            {
                get { return MySqlSession._transactions; }
            }

            private int Level { get; }

            private string Name { get; }

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
                Transactions.Pop();
                _isDisposed = true;
            }

            public sealed override Task CommitAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                return PerformCommitAsync(ct);
            }

            public sealed override Task RollbackAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                return PerformRollbackAsync(ct);
            }

            protected override Task<int> ExecuteNonQueryAsync(MySqlCommand command, CancellationToken ct)
            {
                return PerformExecuteNonQueryAsync(command, ct);
            }

            protected override Task<MySqlReader> ExecuteReaderAsync(Model model, MySqlCommand command, CancellationToken ct)
            {
                return PerformExecuteReaderAsync(model, command, ct);
            }

            private Task PerformCommitAsync(CancellationToken ct)
            {
                return Task.CompletedTask;
            }

            private Task PerformRollbackAsync(CancellationToken ct)
            {
                return Task.CompletedTask;
            }

            private void PerformDispose()
            {
            }
        }

        public sealed override ITransaction BeginTransaction(string name = null)
        {
            return new MySqlTransaction(this, name);
        }

        public sealed override ITransaction BeginTransaction(IsolationLevel isolation, string name = null)
        {
            return new MySqlTransaction(this, name);
        }
    }
}
