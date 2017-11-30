using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        public int Delete(Func<T, _Boolean> where)
        {
            VerifyDelete();
            var statement = BuildDeleteStatement(where);
            return UpdateOrigin(null, DbSession.Delete(statement));
        }

        public async Task<int> DeleteAsync(Func<T, _Boolean> where, CancellationToken cancellationToken)
        {
            VerifyDelete();
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

        public int Delete<TSource>(DbSet<TSource> source, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            VerifyDelete(source);

            var statement = BuildDeleteStatement(source, joinOn);
            return UpdateOrigin(null, DbSession.Update(statement));
        }

        public async Task<int> DeleteAsync<TSource>(DbSet<TSource> source, Func<T, PrimaryKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifyDelete(source);

            var statement = BuildDeleteStatement(source, joinOn);
            return UpdateOrigin(null, await DbSession.DeleteAsync(statement, cancellationToken));
        }

        public Task<int> DeleteAsync<TSource>(DbSet<TSource> source, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(source, joinOn, CancellationToken.None);
        }

        public bool Delete<TSource>(DataSet<TSource> source, int ordinal, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            VerifyDelete(source, ordinal);

            var statement = BuildDeleteScalarStatement(source, ordinal, joinOn);
            return UpdateOrigin<TSource>(null, DbSession.Delete(statement) > 0);
        }

        public async Task<bool> DeleteAsync<TSource>(DataSet<TSource> source,  int ordinal, Func<T, PrimaryKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifyDelete(source, ordinal);

            var statement = BuildDeleteScalarStatement(source, ordinal, joinOn);
            return UpdateOrigin<TSource>(null, await DbSession.DeleteAsync(statement, cancellationToken) > 0);
        }

        public Task<bool> DeleteAsync<TSource>(DataSet<TSource> source, int ordinal, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(source, ordinal, joinOn, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteStatement<TSource>(DbSet<TSource> source, Func<T, PrimaryKey> joinOn)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);

            var keyMappings = GetKeyMappings(source._, joinOn);
            return source.QueryStatement.BuildDeleteStatement(Model, keyMappings);
        }

        public int Delete<TSource>(DataSet<TSource> source, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            VerifyDelete(source);

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return Delete(source, 0) ? 1 : 0;

            return UpdateOrigin(null, DbSession.Delete(source, this, joinOn));
        }

        public async Task<int> DeleteAsync<TSource>(DataSet<TSource> source, Func<T, PrimaryKey> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            VerifyDelete(source);

            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return await DeleteAsync(source, 0, joinOn, cancellationToken) ? 1 : 0;

            return UpdateOrigin(null, await DbSession.DeleteAsync(source, this, joinOn, cancellationToken));
        }

        public Task<int> DeleteAsync<TSource>(DataSet<TSource> source, Func<T, PrimaryKey> joinOn = null)
            where TSource : Model, new()
        {
            return DeleteAsync(source, joinOn, CancellationToken.None);
        }

        internal DbSelectStatement BuildDeleteScalarStatement<TSource>(DataSet<TSource> source, int ordinal, Func<T, PrimaryKey> joinOn)
            where TSource : Model, new()
        {
            Debug.Assert(source != null && source._ != null);
            var sourceModel = source._;
            var keyMappings = GetKeyMappings(sourceModel, joinOn);
            return BuildDeleteScalarStatement(source[ordinal], keyMappings);
        }

        private DbSelectStatement BuildDeleteScalarStatement(DataRow dataRow, IReadOnlyList<ColumnMapping> keyMappings)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, keyMappings), FromClause, keyMappings);
            return new DbSelectStatement(Model, null, from, null, null, -1, -1);
        }

        internal void VerifyDelete()
        {
            if (Model.ChildModels.Any(x => x != null))
                throw new NotSupportedException(Strings.DbTable_DeleteNotSupportedForParentTable);
        }

        internal void VerifyDelete<TSource>(DbSet<TSource> source)
            where TSource : Model, new()
        {
            VerifyDelete();
            VerifySource(source);
        }

        internal void VerifyDelete<TSource>(DataSet<TSource> source)
            where TSource : Model, new()
        {
            VerifyDelete();
            VerifySource(source);
        }

        internal void VerifyDelete<TSource>(DataSet<TSource> source, int ordinal)
            where TSource : Model, new()
        {
            VerifyDelete();
            VerifySource(source, ordinal);
        }
    }
}
