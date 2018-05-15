using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbTableUpdate<T> : Executable<int>
        where T : Model, new()
    {
        protected DbTableUpdate(DbTable<T> target)
        {
            Debug.Assert(target != null);
            _target = target;
        }

        private readonly DbTable<T> _target;
        protected DbTable<T> Target
        {
            get { return _target; }
        }

        protected DbSession DbSession
        {
            get { return Target.DbSession; }
        }

        internal static DbTableUpdate<T> Create(DbTable<T> target, IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where)
        {
            return new UpdateWhere(target, columnMappings, where);
        }

        private sealed class UpdateWhere : DbTableUpdate<T>
        {
            public UpdateWhere(DbTable<T> target, IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where)
                : base(target)
            {
                _columnMappings = columnMappings;
                _where = where;
            }

            private readonly IReadOnlyList<ColumnMapping> _columnMappings;
            private readonly Func<T, _Boolean> _where;

            private DbSelectStatement BuildUpdateStatement()
            {
                return Target.BuildUpdateStatement(_columnMappings, _where);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildUpdateStatement();
                return Target.UpdateOrigin(null, await DbSession.UpdateAsync(statement, ct));
            }
        }

        internal static DbTableUpdate<T> Create<TSource>(DbTable<T> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            return new UpdateFromDbSet<TSource>(target, source, columnMappings, join);
        }

        private sealed class UpdateFromDbSet<TSource> : DbTableUpdate<T>
            where TSource : Model, new()
        {
            public UpdateFromDbSet(DbTable<T> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
                : base(target)
            {
                _source = source;
                _columnMappings = columnMappings;
                _join = join;
            }

            private readonly DbSet<TSource> _source;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;
            private readonly IReadOnlyList<ColumnMapping> _join;

            private DbSelectStatement BuildUpdateStatement()
            {
                return Target.BuildUpdateStatement(_source, _columnMappings, _join);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildUpdateStatement();
                return Target.UpdateOrigin(null, await DbSession.UpdateAsync(statement, ct));
            }
        }

        internal static DbTableUpdate<T> Create<TSource>(DbTable<T> target, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            return new UpdateFromDataRow<TSource>(target, source, rowIndex, columnMappings, join);
        }

        private sealed class UpdateFromDataRow<TSource> : DbTableUpdate<T>
            where TSource : Model, new()
        {
            public UpdateFromDataRow(DbTable<T> target, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
                : base(target)
            {
                _source = source;
                _rowIndex = rowIndex;
                _columnMappings = columnMappings;
                _join = join;
            }

            private readonly DataSet<TSource> _source;
            private readonly int _rowIndex;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;
            private readonly IReadOnlyList<ColumnMapping> _join;

            private DbSelectStatement BuildUpdateStatement()
            {
                return Target.BuildUpdateScalarStatement(_source, _rowIndex, _columnMappings, _join);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildUpdateStatement();
                return Target.UpdateOrigin<TSource>(null, await DbSession.UpdateAsync(statement, ct) > 0) ? 1 : 0;
            }
        }

        internal static DbTableUpdate<T> Create<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo)
            where TSource : Model, new()
        {
            return new UpdateFromDataSet<TSource>(target, source, columnMapper, joinTo);
        }

        private sealed class UpdateFromDataSet<TSource> : DbTableUpdate<T>
            where TSource : Model, new()
        {
            public UpdateFromDataSet(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo)
                : base(target)
            {
                Debug.Assert(source.Count != 1);
                _source = source;
                _columnMapper = columnMapper;
                _joinTo = joinTo;
            }

            private readonly DataSet<TSource> _source;
            private readonly Action<ColumnMapper, TSource, T> _columnMapper;
            private readonly PrimaryKey _joinTo;

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                if (_source.Count == 0)
                    return 0;
                return await DbSession.UpdateAsync(_source, Target, _columnMapper, _joinTo, ct);
            }
        }
    }
}
