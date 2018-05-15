using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbTableDelete<T> : Executable<int>
        where T : Model, new()
    {
        internal static DbTableDelete<T> Create(DbTable<T> from, Func<T, _Boolean> where)
        {
            return new DeleteWhere(from, where);
        }

        internal static DbTableDelete<T> Create<TSource>(DbTable<T> from, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : Model, new()
        {
            return new DeleteFromDbSet<TSource>(from, source, columnMappings);
        }

        internal static DbTableDelete<T> Create<TSource>(DbTable<T> from, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : Model, new()
        {
            return new DeleteFromDataRow<TSource>(from, source, rowIndex, columnMappings);
        }

        internal static DbTableDelete<T> Create<TSource>(DbTable<T> from, DataSet<TSource> source, PrimaryKey joinTo)
            where TSource : Model, new()
        {
            return new DeleteFromDataSet<TSource>(from, source, joinTo);
        }

        protected DbTableDelete(DbTable<T> from)
        {
            Debug.Assert(from != null);
            _from = from;
        }

        private readonly DbTable<T> _from;
        protected DbTable<T> From
        {
            get { return _from; }
        }

        protected DbSession DbSession
        {
            get { return From.DbSession; }
        }

        private sealed class DeleteWhere : DbTableDelete<T>
        {
            public DeleteWhere(DbTable<T> from, Func<T, _Boolean> where)
                : base(from)
            {
                _where = where;
            }

            private readonly Func<T, _Boolean> _where;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteStatement(_where);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, await DbSession.DeleteAsync(statement, ct));
            }
        }
            

        private sealed class DeleteFromDbSet<TSource> : DbTableDelete<T>
            where TSource : Model, new()
        {
            public DeleteFromDbSet(DbTable<T> from, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings)
                : base(from)
            {
                _source = source;
                _columnMappings = columnMappings;
            }

            private readonly DbSet<TSource> _source;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteStatement(_source, _columnMappings);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, await DbSession.DeleteAsync(statement, ct));
            }
        }

        private sealed class DeleteFromDataRow<TSource> : DbTableDelete<T>
            where TSource : Model, new()
        {
            public DeleteFromDataRow(DbTable<T> from, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings)
                : base(from)
            {
                _source = source;
                _rowIndex = rowIndex;
                _columnMappings = columnMappings;
            }

            private readonly DataSet<TSource> _source;
            private readonly int _rowIndex;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteScalarStatement(_source, _rowIndex, _columnMappings);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin<TSource>(null, await DbSession.DeleteAsync(statement, ct) > 0) ? 1 : 0;
            }
        }

        private sealed class DeleteFromDataSet<TSource> : DbTableDelete<T>
            where TSource : Model, new()
        {
            public DeleteFromDataSet(DbTable<T> from, DataSet<TSource> source, PrimaryKey joinTo)
                : base(from)
            {
                Debug.Assert(source.Count != 1);
                _source = source;
                _joinTo = joinTo;
            }

            private readonly DataSet<TSource> _source;
            private readonly PrimaryKey _joinTo;

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                if (_source.Count == 0)
                    return 0;
                return await DbSession.DeleteAsync(_source, From, _joinTo, ct);
            }
        }
    }
}
