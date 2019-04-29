using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using DevZest.Data.Primitives;

namespace DevZest.Data.SqlServer
{
    partial class SqlSession
    {
        private readonly Stack<SqlTransaction> _transactions = new Stack<SqlTransaction>();

        public sealed override int TransactionCount
        {
            get { return _transactions.Count; }
        }

        protected sealed override Transaction CurrentTransaction
        {
            get { return _transactions.Count == 0 ? null : _transactions.Peek(); }
        }

        private sealed class SqlTransaction : Transaction
        {
            public SqlTransaction(SqlSession sqlSession, string name)
            {
                SqlSession = sqlSession;
                Level = sqlSession.TransactionCount;
                Name = name;
                Transactions.Push(this);
            }

            private bool IsCurrent
            {
                get { return SqlSession.CurrentTransaction == this; }
            }

            private void VerifyIsCurrent()
            {
                if (!IsCurrent)
                    throw new InvalidOperationException("Operation only allowed for current transaction.");
            }

            private SqlSession SqlSession { get; }

            protected sealed override DbSession<SqlConnection, SqlCommand, SqlReader> GetDbSession()
            {
                return SqlSession;
            }

            private Stack<SqlTransaction> Transactions
            {
                get { return SqlSession._transactions; }
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

            protected override Task<int> ExecuteNonQueryAsync(SqlCommand command, CancellationToken ct)
            {
                return PerformExecuteNonQueryAsync(command, ct);
            }

            protected override Task<SqlReader> ExecuteReaderAsync(Model model, SqlCommand command, CancellationToken ct)
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
            return new SqlTransaction(this, name);
        }
    }
}
