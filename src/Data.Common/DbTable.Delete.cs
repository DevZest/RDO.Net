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
        public int Delete(Func<T, _Boolean> getWhere = null)
        {
            var statement = BuildDeleteStatement(getWhere);
            return DbSession.Delete(statement);
        }

        public Task<int> DeleteAsync(Func<T, _Boolean> getWhere, CancellationToken cancellationToken)
        {
            var statement = BuildDeleteStatement(getWhere);
            return DbSession.DeleteAsync(statement, cancellationToken);
        }

        public Task<int> DeleteAsync(Func<T, _Boolean> getWhere = null)
        {
            return DeleteAsync(getWhere, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteStatement(Func<T, _Boolean> getWhere)
        {
            var where = VerifyWhere(getWhere);
            return new DbSelectStatement(this._, null, null, where, null, -1, -1);
        }

        public int Delete<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder = null)
            where TSource : Model, new()
        {
            var statement = BuildDeleteStatement(dbSet, keyMappingsBuilder);
            return DbSession.Update(statement);
        }

        public Task<int> DeleteAsync<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var statement = BuildDeleteStatement(dbSet, keyMappingsBuilder);
            return DbSession.DeleteAsync(statement, cancellationToken);
        }

        public Task<int> DeleteAsync<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder = null)
            where TSource : Model, new()
        {
            return DeleteAsync(dbSet, keyMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteStatement<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder)
            where TSource : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            var keyMappings = keyMappingsBuilder == null ? GetKeyMappings(dbSet._) : BuildColumnMappings(keyMappingsBuilder, dbSet._);
            return dbSet.QueryStatement.BuildDeleteStatement(this, keyMappings);
        }

        public int Delete<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder = null)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));

            if (dataSet.Count == 0)
                return 0;

            if (dataSet.Count == 1)
                return DbSession.Update(BuildDeleteScalarStatement(dataSet, 0, keyMappingsBuilder));

            return Delete(DbSession.ImportDataSet(dataSet), keyMappingsBuilder);
        }

        public async Task<int> DeleteAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));

            if (dataSet.Count == 0)
                return 0;

            if (dataSet.Count == 1)
                return await DbSession.UpdateAsync(BuildDeleteScalarStatement(dataSet, 0, keyMappingsBuilder), cancellationToken);

            return await DeleteAsync(await DbSession.ImportDataSetAsync(dataSet, cancellationToken), keyMappingsBuilder, cancellationToken);
        }

        public Task<int> DeleteAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder = null)
            where TSource : Model, new()
        {
            return DeleteAsync(dataSet, keyMappingsBuilder, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, Action<ColumnMappingsBuilder, T, TSource> keyMappingsBuilder)
            where TSource : Model, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            var sourceModel = dataSet._;
            var keyMappings = keyMappingsBuilder == null ? GetKeyMappings(sourceModel) : BuildColumnMappings(keyMappingsBuilder, sourceModel);
            return BuildDeleteScalarStatement(dataSet[ordinal], keyMappings);
        }

        private DbSelectStatement BuildDeleteScalarStatement(DataRow dataRow, IList<ColumnMapping> keyMappings)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, keyMappings), FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
            return new DbSelectStatement(this._, null, from, null, null, -1, -1);
        }
    }
}
