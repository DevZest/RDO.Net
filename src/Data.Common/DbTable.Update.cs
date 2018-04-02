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
        public DbUpdate<T> Update(Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where = null)
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbUpdate<T>.Create(this, columnMappings, where);
        }

        internal DbSelectStatement BuildUpdateStatement(IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where)
        {
            Debug.Assert(columnMappings != null && columnMappings.Count > 0);
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, columnMappings, null, whereExpr, null, -1, -1);
        }

        public DbUpdate<T> Update(DbSet<T> source)
        {
            return Update(source, ColumnMapper.InferUpdate, KeyMapping.Infer);
        }

        public DbUpdate<T> Update<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> join)
            where TSource : Model, new()
        {
            Verify(source, nameof(source));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var keyMapping = Verify(join, nameof(join), source._);
            return DbUpdate<T>.Create(this, source, columnMappings, keyMapping.GetColumnMappings());
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, join);
        }

        // TODO: This should be deleted.
        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMappingsBuilder)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);
            var keyMappings = GetKeyMappings(source._, null);
            var columnMappings = GetColumnMappings(source._, columnMappingsBuilder, false);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, keyMappings);
        }

        public DbUpdate<T> Update(DataSet<T> source, int rowIndex)
        {
            return Update(source, rowIndex, ColumnMapper.InferUpdate, KeyMapping.Infer);
        }

        public DbUpdate<T> Update<TSource>(DataSet<TSource> source, int rowIndex, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper)
            where TSource : Model, new()
        {
            Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var join = Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            return DbUpdate<T>.Create(this, source, rowIndex, columnMappings, join);
        }

        public int Update<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            Verify(source, nameof(source));

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                throw new NotImplementedException();

            return UpdateOrigin(null, DbSession.Update(source, this, columnMappingsBuilder));
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            return UpdateAsync(source, null, cancellationToken);
        }


        public async Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Verify(source, nameof(source));

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                throw new NotImplementedException();

            return UpdateOrigin(null, await DbSession.UpdateAsync(source, this, columnMappingsBuilder, cancellationToken));
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(source, columnMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
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
