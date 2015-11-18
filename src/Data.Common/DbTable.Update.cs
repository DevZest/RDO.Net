using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        public int Update(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> where = null)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var statement = BuildUpdateStatement(columnMappingsBuilder, where);
            return UpdateOrigin(null, DbSession.Update(statement));
        }

        public async Task<int> UpdateAsync(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> where, CancellationToken cancellationToken)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var statement = BuildUpdateStatement(columnMappingsBuilder, where);
            return UpdateOrigin(null, await DbSession.UpdateAsync(statement, cancellationToken));
        }

        public Task<int> UpdateAsync(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> where = null)
        {
            return UpdateAsync(columnMappingsBuilder, where, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateStatement(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> where)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var columnMappings = BuildColumnMappings(columnMappingsBuilder);
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, columnMappings, null, whereExpr, null, -1, -1);
        }

        public int Update<TSource>(DbSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            VerifySource(source);

            var statement = BuildUpdateStatement(source, columnMappingsBuilder);
            return UpdateOrigin(null, DbSession.Update(statement));
        }

        public async Task<int> UpdateAsync<TSource>(DbSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifySource(source);

            var statement = BuildUpdateStatement(source, columnMappingsBuilder);
            return UpdateOrigin(null, await DbSession.UpdateAsync(statement, cancellationToken));
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(source, columnMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);
            var keyMappings = GetKeyMappings(source._);
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(source._) : _.BuildColumnMappings(source._, columnMappingsBuilder);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, keyMappings);
        }

        public bool Update<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            VerifySource(source, ordinal);

            var statement = BuildUpdateScalarStatement(source, ordinal, columnMappingsBuilder);
            var result = DbSession.Update(statement);
            UpdateOrigin(null, result);
            return result == 1;
        }

        public async Task<bool> UpdateAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder,
            CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifySource(source, ordinal);

            var statement = BuildUpdateScalarStatement(source, ordinal, columnMappingsBuilder);
            var result = await DbSession.UpdateAsync(statement, cancellationToken);
            UpdateOrigin(null, result);
            return result == 1;
        }

        public Task<bool> UpdateAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(source, ordinal, columnMappingsBuilder, CancellationToken.None);
        }

        public int Update<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            VerifySource(source);

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return Update(source, 0, columnMappingsBuilder) ? 1 : 0;

            return UpdateOrigin(null, DbSession.Update(source, this, columnMappingsBuilder));
        }

        public async Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifySource(source);

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return await UpdateAsync(source, 0, columnMappingsBuilder, cancellationToken) ? 1 : 0;

            return UpdateOrigin(null, await DbSession.UpdateAsync(source, this, columnMappingsBuilder, cancellationToken));
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(source, columnMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder)
            where TSource : Model, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            var sourceModel = dataSet._;
            var keyMappings = GetKeyMappings(sourceModel);
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : _.BuildColumnMappings(sourceModel, columnMappingsBuilder);
            return BuildUpdateScalarStatement(dataSet[ordinal], keyMappings, columnMappings);
        }

        private DbSelectStatement BuildUpdateScalarStatement(DataRow dataRow, IList<ColumnMapping> keyMappings, IList<ColumnMapping> columnMappings)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var select = GetScalarMapping(paramManager, columnMappings);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, keyMappings), FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
            return new DbSelectStatement(Model, select, from, null, null, -1, -1);
        }
    }
}
