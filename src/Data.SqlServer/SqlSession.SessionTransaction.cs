﻿using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using DevZest.Data.Primitives;
using System.Data;

namespace DevZest.Data.SqlServer
{
    partial class SqlSession
    {
        private readonly Stack<SessionTransaction> _transactions = new Stack<SessionTransaction>();

        public sealed override int TransactionCount
        {
            get { return _transactions.Count; }
        }

        protected sealed override Transaction CurrentTransaction
        {
            get { return GetCurrentTransaction(); }
        }

        private SessionTransaction GetCurrentTransaction()
        {
            return _transactions.Count == 0 ? null : _transactions.Peek();
        }

        private abstract class SessionTransaction : Transaction
        {
            public static SessionTransaction Create(SqlSession sqlSession, string name)
            {
                if (sqlSession.TransactionCount == 0)
                    return new Transaction(sqlSession, sqlSession.Connection.BeginTransaction(name), name);
                else
                    return new SavePoint(sqlSession, sqlSession.GetCurrentTransaction().SqlTransaction, name);
            }

            public static SessionTransaction Create(SqlSession sqlSession, IsolationLevel isolation, string name)
            {
                if (sqlSession.TransactionCount == 0)
                    return new Transaction(sqlSession, sqlSession.Connection.BeginTransaction(isolation, name), name);
                else
                    return new SavePoint(sqlSession, sqlSession.GetCurrentTransaction().SqlTransaction, name);
            }

            private sealed class Transaction : SessionTransaction
            {
                public Transaction(SqlSession sqlSession, SqlTransaction sqlTransaction, string name)
                    : base(sqlSession, sqlTransaction, name)
                {
                }

                protected override void Commit()
                {
                    SqlTransaction.Commit();
                }

                protected override void Rollback()
                {
                    SqlTransaction.Rollback();
                }
            }

            private sealed class SavePoint : SessionTransaction
            {
                public SavePoint(SqlSession sqlSession, SqlTransaction sqlTransaction, string name)
                    : base(sqlSession, sqlTransaction, name)
                {
                    sqlTransaction.Save(Name);
                }

                protected override void Commit()
                {
                }

                protected override void Rollback()
                {
                    SqlTransaction.Rollback(Name);
                }
            }


            protected SessionTransaction(SqlSession sqlSession, SqlTransaction sqlTransaction, string name)
            {
                SqlSession = sqlSession;
                SqlTransaction = sqlTransaction;
                Level = sqlSession.TransactionCount;
                Name = GetName(name);
                Transactions.Push(this);
            }

            public SqlTransaction SqlTransaction { get; }

            private string GetName(string name)
            {
                if (string.IsNullOrEmpty(name))
                    name = "_SYS_XACT";
                return string.Format("{0}_{1}", name, Level);
            }

            private bool IsCurrent
            {
                get { return SqlSession.CurrentTransaction == this; }
            }

            private void VerifyIsCurrent()
            {
                if (!IsCurrent)
                    throw new InvalidOperationException(DiagnosticMessages.VerifyIsCurrentTransaction);
            }

            private SqlSession SqlSession { get; }

            protected sealed override DbSession<SqlConnection, SqlCommand, SqlReader> GetDbSession()
            {
                return SqlSession;
            }

            private Stack<SessionTransaction> Transactions
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
                VerifyNotFrozen();
                Commit();
                _isFrozen = true;
                return Task.CompletedTask;
            }

            public sealed override Task RollbackAsync(CancellationToken ct)
            {
                VerifyIsCurrent();
                VerifyNotFrozen();
                Rollback();
                _isFrozen = true;
                return Task.CompletedTask;
            }

            protected override Task<int> ExecuteNonQueryAsync(SqlCommand command, CancellationToken ct)
            {
                VerifyNotFrozen();
                command.Transaction = SqlTransaction;
                return PerformExecuteNonQueryAsync(command, ct);
            }

            protected override Task<SqlReader> ExecuteReaderAsync(Model model, SqlCommand command, CancellationToken ct)
            {
                VerifyNotFrozen();
                command.Transaction = SqlTransaction;
                return PerformExecuteReaderAsync(model, command, ct);
            }

            private bool _isFrozen;

            private void VerifyNotFrozen()
            {
                if (_isFrozen)
                    throw new InvalidOperationException();
            }

            protected abstract void Commit();

            protected abstract void Rollback();

            private void PerformDispose()
            {
                if (_isFrozen)
                    return;

                Rollback();
            }
        }

        public sealed override ITransaction BeginTransaction(string name = null)
        {
            return SessionTransaction.Create(this, name);
        }

        public sealed override ITransaction BeginTransaction(IsolationLevel isolation, string name = null)
        {
            return SessionTransaction.Create(this, isolation, name);
        }
    }
}