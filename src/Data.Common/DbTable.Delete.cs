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
        public int Delete(Func<T, _Boolean> where)
        {
            var statement = BuildDeleteStatement(where);
            return UpdateOrigin(null, DbSession.Delete(statement));
        }

        public async Task<int> DeleteAsync(Func<T, _Boolean> where, CancellationToken cancellationToken)
        {
            var statement = BuildDeleteStatement(where);
            return UpdateOrigin(null, await DbSession.DeleteAsync(statement, cancellationToken));
        }

        public Task<int> DeleteAsync(Func<T, _Boolean> where)
        {
            return DeleteAsync(where, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteStatement(Func<T, _Boolean> where)
        {
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, null, null, whereExpr, null, -1, -1);
        }

        public int Delete<TSource>(DbSet<TSource> source, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            var statement = BuildDeleteStatement(source, joinOn);
            return UpdateOrigin(null, DbSession.Update(statement));
        }

        public async Task<int> DeleteAsync<TSource>(DbSet<TSource> source, Func<T, ModelKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var statement = BuildDeleteStatement(source, joinOn);
            return UpdateOrigin(null, await DbSession.DeleteAsync(statement, cancellationToken));
        }

        public Task<int> DeleteAsync<TSource>(DbSet<TSource> source, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(source, joinOn, CancellationToken.None);
        }

        public bool Delete<TSource>(DataSet<TSource> source, int ordinal, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));

            var statement = BuildDeleteScalarStatement(source, ordinal, joinOn);
            return UpdateOrigin<TSource>(null, DbSession.Delete(statement) > 0);
        }

        public async Task<bool> DeleteAsync<TSource>(DataSet<TSource> source,  int ordinal, Func<T, ModelKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));

            var statement = BuildDeleteScalarStatement(source, ordinal, joinOn);
            return UpdateOrigin<TSource>(null, await DbSession.DeleteAsync(statement, cancellationToken) > 0);
        }

        public Task<bool> DeleteAsync<TSource>(DataSet<TSource> source, int ordinal, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(source, ordinal, joinOn, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteStatement<TSource>(DbSet<TSource> source, Func<T, ModelKey> joinOn)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            var keyMappings = GetKeyMappings(source._);
            return source.QueryStatement.BuildDeleteStatement(Model, keyMappings);
        }

        public int Delete<TSource>(DataSet<TSource> source, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return Delete(source, 0) ? 1 : 0;

            return UpdateOrigin(null, DbSession.Delete(source, this, joinOn));
        }

        public async Task<int> DeleteAsync<TSource>(DataSet<TSource> source, Func<T, ModelKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return await DeleteAsync(source, 0, joinOn, cancellationToken) ? 1 : 0;

            return UpdateOrigin(null, await DbSession.DeleteAsync(source, this, joinOn, cancellationToken));
        }

        public Task<int> DeleteAsync<TSource>(DataSet<TSource> dataSet, Func<T, ModelKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(dataSet, joinOn, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, Func<T, ModelKey> joinOn)
            where TSource : Model, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            var sourceModel = dataSet._;
            var keyMappings = GetKeyMappings(sourceModel, joinOn);
            return BuildDeleteScalarStatement(dataSet[ordinal], keyMappings);
        }

        private DbSelectStatement BuildDeleteScalarStatement(DataRow dataRow, IList<ColumnMapping> keyMappings)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, keyMappings), FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
            return new DbSelectStatement(Model, null, from, null, null, -1, -1);
        }
    }
}
