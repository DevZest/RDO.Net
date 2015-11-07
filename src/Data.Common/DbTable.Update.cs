﻿using DevZest.Data.Primitives;
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
        public int Update(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> getWhere = null)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var statement = BuildUpdateStatement(columnMappingsBuilder, getWhere);
            return DbSession.Update(statement);
        }

        public Task<int> UpdateAsync(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> getWhere, CancellationToken cancellationToken)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var statement = BuildUpdateStatement(columnMappingsBuilder, getWhere);
            return DbSession.UpdateAsync(statement, cancellationToken);
        }

        public Task<int> UpdateAsync(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> getWhere = null)
        {
            return UpdateAsync(columnMappingsBuilder, getWhere, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateStatement(Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> getWhere)
        {
            Check.NotNull(columnMappingsBuilder, nameof(columnMappingsBuilder));
            var columnMappings = BuildColumnMappings(columnMappingsBuilder);
            var where = VerifyWhere(getWhere);
            return new DbSelectStatement(this._, columnMappings, null, where, null, -1, -1);
        }

        public int Update<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            var statement = BuildUpdateStatement(dbSet, columnMappingsBuilder);
            return DbSession.Update(statement);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var statement = BuildUpdateStatement(dbSet, columnMappingsBuilder);
            return DbSession.UpdateAsync(statement, cancellationToken);
        }

        public Task<int> UpdateAsync<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(dbSet, columnMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder)
            where TSource : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            var keyMappings = GetKeyMappings(dbSet._);
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(dbSet._) : BuildColumnMappings(columnMappingsBuilder, dbSet._);
            return dbSet.QueryStatement.BuildUpdateStatement(this, keyMappings, columnMappings);
        }

        public int Update<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));

            if (dataSet.Count == 0)
                return 0;

            if (dataSet.Count == 1)
                return DbSession.Update(BuildUpdateScalarStatement(dataSet, 0, columnMappingsBuilder));

            return Update(DbSession.Import(dataSet), columnMappingsBuilder);
        }

        public async Task<int> UpdateAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));

            if (dataSet.Count == 0)
                return 0;

            if (dataSet.Count == 1)
                return await DbSession.UpdateAsync(BuildUpdateScalarStatement(dataSet, 0, columnMappingsBuilder), cancellationToken);

            return await UpdateAsync(await DbSession.ImportAsync(dataSet, cancellationToken), columnMappingsBuilder, cancellationToken);
        }

        public Task<int> UpdateAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder = null)
            where TSource : Model, new()
        {
            return UpdateAsync(dataSet, columnMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildUpdateScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder)
            where TSource : Model, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            var sourceModel = dataSet._;
            var keyMappings = GetKeyMappings(sourceModel);
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : BuildColumnMappings(columnMappingsBuilder, sourceModel);
            return BuildUpdateScalarStatement(dataSet[ordinal], keyMappings, columnMappings);
        }

        private DbSelectStatement BuildUpdateScalarStatement(DataRow dataRow, IList<ColumnMapping> keyMappings, IList<ColumnMapping> columnMappings)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var select = GetScalarMapping(paramManager, columnMappings);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, keyMappings), FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
            return new DbSelectStatement(this._, select, from, null, null, -1, -1);
        }
    }
}
