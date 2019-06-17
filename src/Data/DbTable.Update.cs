using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        public Task<int> UpdateAsync(Action<ColumnMapper, T> columnMapper, CancellationToken ct = default(CancellationToken))
        {
            return UpdateAsync(columnMapper, null, ct);
        }

        public Task<int> UpdateAsync(Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where, CancellationToken ct = default(CancellationToken))
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableUpdate<T>.ExecuteAsync(this, columnMappings, where, ct);
        }

        internal DbSelectStatement BuildUpdateStatement(IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where)
        {
            Debug.Assert(columnMappings != null && columnMappings.Count > 0);
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, columnMappings, null, whereExpr, null, -1, -1);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return UpdateAsync(source, (m, s, t) => ColumnMapper.AutoSelectUpdatable(m, s, t), KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> source, Action<ColumnMapper, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource: class, T, new()
        {
            return UpdateAsync(source, (m, s, t) => columnMapper(m, t), KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return UpdateAsync(source, columnMapper, KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> join, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var keyMapping = Verify(join, nameof(join), source._);
            return DbTableUpdate<T>.ExecuteAsync(this, source, columnMappings, keyMapping.GetColumnMappings(), ct);
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
        {
            Debug.Assert(source != null);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, join);
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
        {
            Debug.Assert(source != null);
            Debug.Assert(columnMapper != null);
            var columnMappings = Verify(columnMapper, source._);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, join);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, int rowIndex, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return UpdateAsync(source, rowIndex, (m, s, t) => ColumnMapper.AutoSelectUpdatable(m, s, t), KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, int rowIndex, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var join = Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            return DbTableUpdate<T>.ExecuteAsync(this, source, rowIndex, columnMappings, join, ct);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return UpdateAsync(source, (m, s, t) => ColumnMapper.AutoSelectUpdatable(m, s, t), KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return UpdateAsync(source, columnMapper, KeyMapping.Match, ct);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return UpdateAsync(source, 0, columnMapper, joinMapper, ct);

            Verify(columnMapper, nameof(columnMapper));
            var joinTo = Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            return DbTableUpdate<T>.ExecuteAsync(this, source, columnMapper, joinTo, ct);
        }

        internal DbSelectStatement BuildUpdateScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            return BuildUpdateScalarStatement(dataSet[ordinal], columnMappings, join);
        }

        private DbSelectStatement BuildUpdateScalarStatement(DataRow dataRow, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var select = GetScalarMapping(paramManager, columnMappings);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, join), FromClause, join);
            return new DbSelectStatement(Model, select, from, null, null, -1, -1);
        }
    }
}
