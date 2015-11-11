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
        public int Insert<TSource>(DbQuery<TSource> dbQuery, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
        {
            Check.NotNull(dbQuery, nameof(dbQuery));
            return dbQuery.DbSession.Insert(BuildInsertStatement(dbQuery, columnMappingsBuilder, autoJoin));
        }

        public Task<int> InsertAsync<TSource>(DbQuery<TSource> dbQuery, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dbQuery, nameof(dbQuery));
            return dbQuery.DbSession.InsertAsync(BuildInsertStatement(dbQuery, columnMappingsBuilder, autoJoin), cancellationToken);
        }

        public Task<int> InsertAsync<TSource>(DbQuery<TSource> dbQuery, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
        {
            return InsertAsync(dbQuery, columnMappingsBuilder, autoJoin, CancellationToken.None);
        }

        public int Insert<TSource>(DbTable<TSource> dbTable, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            Check.NotNull(dbTable, nameof(dbTable));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = InsertTable(dbTable, columnMappingsBuilder, autoJoin, updateIdentity);
            UpdateIdentity(dbTable, result);
            return result.RowCount;
        }

        public async Task<int> InsertAsync<TSource>(DbTable<TSource> dbTable, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dbTable, nameof(dbTable));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = await InsertTableAsync(dbTable, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken);
            await UpdateIdentityAsync(dbTable, result, cancellationToken);
            return result.RowCount;
        }

        public Task<int> InsertAsync<TSource>(DbTable<TSource> dbTable, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            return InsertAsync(dbTable, columnMappingsBuilder, autoJoin, updateIdentity, CancellationToken.None);
        }

        private void VerifyUpdateIdentity(bool updateIdentity, string paramName)
        {
            if (!updateIdentity)
                return;

            if (Kind == DataSourceKind.DbTempTable || Model.GetIdentity(false) == null)
                throw new ArgumentException(Strings.DbTable_VerifyUpdateIdentity, paramName);
        }

        private struct InsertTableResult
        {
            public InsertTableResult(int rowCount, DbTable<IdentityMapping> identityMappings)
            {
                RowCount = rowCount;
                IdentityMappings = identityMappings;
            }

            public readonly int RowCount;

            public readonly DbTable<IdentityMapping> IdentityMappings;
        }

        private InsertTableResult InsertTable<TSource>(DbTable<TSource> dbTable, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? DbSession.CreateTempTable<IdentityMapping>() : null;
            var rowCount = DbSession.Insert(dbTable, this, columnMappingsBuilder, autoJoin, identityMappings);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> InsertTableAsync<TSource>(DbTable<TSource> dbTable, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? await DbSession.CreateTempTableAsync<IdentityMapping>(null, cancellationToken) : null;
            var rowCount = await DbSession.InsertAsync(dbTable, this, columnMappingsBuilder, autoJoin, identityMappings, cancellationToken);
            return new InsertTableResult(rowCount, identityMappings);
        }

        internal DbSelectStatement BuildInsertStatement<TSource>(DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
        {
            var sourceModel = dbSet._;
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : _.BuildColumnMappings(sourceModel, columnMappingsBuilder);
            var keyMappings = autoJoin ? GetKeyMappings(sourceModel) : null;
            return dbSet.QueryStatement.BuildInsertStatement(Model, columnMappings, keyMappings);
        }

        public int Insert<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return InsertDataSet(dataSet, columnMappingsBuilder, autoJoin, updateIdentity);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return InsertDataSetAsync(dataSet, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            return InsertAsync(dataSet, columnMappingsBuilder, autoJoin, updateIdentity, CancellationToken.None);
        }

        private int InsertDataSet<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            if (dataSet.Count == 0)
                return 0;
            else if (dataSet.Count == 1)
                return Insert(dataSet, 0, columnMappingsBuilder, autoJoin, updateIdentity) ? 1 : 0;
            else
            {
                if (!updateIdentity)
                    return DbSession.Insert(dataSet, this, columnMappingsBuilder, autoJoin);

                var tempTable = dataSet.ToTempTable(DbSession);
                var result = InsertTable(tempTable, columnMappingsBuilder, autoJoin, updateIdentity);
                UpdateIdentity(dataSet, result);
                return result.RowCount;
            }
        }

        private async Task<int> InsertDataSetAsync<TSource>(DataSet<TSource> dataSet, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            if (dataSet.Count == 0)
                return 0;
            else if (dataSet.Count == 1)
                return await InsertAsync(dataSet, 0, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken) ? 1 : 0;
            else
            {
                if (!updateIdentity)
                    return await DbSession.InsertAsync(dataSet, this, columnMappingsBuilder, autoJoin, cancellationToken);

                var tempTable = await dataSet.ToTempTableAsync(DbSession, cancellationToken);
                var result = await InsertTableAsync(tempTable, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken);
                UpdateIdentity(dataSet, result);
                return result.RowCount;
            }
        }

        public bool Insert<TSource>(DataSet<TSource> dataSet, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var statement = BuildInsertScalarStatement(dataSet, rowOrdinal, columnMappingsBuilder, autoJoin);
            var result = DbSession.InsertScalar(statement, updateIdentity);
            if (updateIdentity)
                UpdateIdentity(dataSet, rowOrdinal, result.IdentityValue);
            return result.Success;
        }

        public async Task<bool> InsertAsync<TSource>(DataSet<TSource> dataSet, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(dataSet, nameof(dataSet));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var statement = BuildInsertScalarStatement(dataSet, rowOrdinal, columnMappingsBuilder, autoJoin);
            var result = await DbSession.InsertScalarAsync(statement, updateIdentity, cancellationToken);
            if (updateIdentity)
                UpdateIdentity(dataSet, rowOrdinal, result.IdentityValue);
            return result.Success;
        }

        internal DbSelectStatement BuildInsertScalarStatement<TSource>(DataSet<TSource> dataSet, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
        {
            var sourceModel = dataSet._;
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : _.BuildColumnMappings(sourceModel, columnMappingsBuilder);
            var keyMappings = autoJoin ? GetKeyMappings(sourceModel) : null;
            var parentMappings = columnMappings.GetParentRelationship(this);

            var paramManager = new ScalarParamManager(dataSet[rowOrdinal]);
            var select = GetScalarMapping(paramManager, columnMappings);
            IDbTable parentTable = null;
            if (parentMappings != null)
            {
                parentTable = (IDbTable)_.ParentModel.DataSource;
                Debug.Assert(parentTable != null);
                var parentRowIdMapping = new ColumnMapping(Model.GetSysParentRowIdColumn(createIfNotExist: false),
                    parentTable.Model.GetSysRowIdColumn(createIfNotExist: false));
                select = select.Append(parentRowIdMapping);
            }

            DbFromClause from = GetScalarDataSource(paramManager, keyMappings, parentMappings);
            DbExpression where = null;
            if (from != null)
            {
                if (parentMappings != null)
                    from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentTable.FromClause, new ReadOnlyCollection<ColumnMapping>(parentMappings));

                if (keyMappings != null)
                {
                    from = new DbJoinClause(DbJoinKind.LeftJoin, from, FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
                    where = new DbFunctionExpression(FunctionKeys.IsNull, new DbExpression[] { keyMappings[0].TargetColumn.DbExpression });
                }
            }

            return new DbSelectStatement(this._, select, from, where, null, -1, -1);
        }

        private void UpdateIdentity<TSource>(DataSet<TSource> dataSet, InsertTableResult result)
            where TSource : Model, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityMappings = result.IdentityMappings;
            var identityColumn = (_Int32)dataSet._.Columns[this._.GetIdentity(false).Column.Ordinal];
            var model = dataSet.Model;
            var oldValue = model.AllowsKeyUpdate(true);
            using (var reader = DbSession.ExecuteDbReader(identityMappings))
            {
                int ordinal = 0;
                while (reader.Read())
                {
                    identityColumn[ordinal++] = identityMappings._.NewValue[reader];
                }
            }
            model.AllowsKeyUpdate(oldValue);
        }

        private async Task UpdateIdentityAsync<TSource>(DataSet<TSource> dataSet, InsertTableResult result, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityOutput = result.IdentityMappings;
            var identityColumn = (_Int32)dataSet._.Columns[this._.GetIdentity(false).Column.Ordinal];
            var model = dataSet.Model;
            var oldValue = model.AllowsKeyUpdate(true);
            using (var reader = await DbSession.ExecuteDbReaderAsync(identityOutput, cancellationToken))
            {
                int ordinal = 0;
                while (await reader.ReadAsync(cancellationToken))
                {
                    identityColumn[ordinal++] = identityOutput._.NewValue[reader];
                }
            }
            model.AllowsKeyUpdate(oldValue);
        }

        private static DbSelectStatement BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : Model, new()
        {
            var identityMappings = result.IdentityMappings;
            if (identityMappings == null || result.RowCount == 0)
                return null;

            return BuildUpdateIdentityStatement(dbTable, identityMappings);
        }

        internal static DbSelectStatement BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, DbTable<IdentityMapping> identityMappings)
            where TSource : Model, new()
        {
            var identityColumn = dbTable._.GetIdentity(false).Column;
            Debug.Assert(!object.ReferenceEquals(identityColumn, null));
            var keyMappings = new ColumnMapping[] { new ColumnMapping(identityMappings._.OldValue, identityColumn) };
            var columnMappings = new ColumnMapping[] { new ColumnMapping(identityMappings._.NewValue, identityColumn) };
            return identityMappings.QueryStatement.BuildUpdateStatement(dbTable, keyMappings, columnMappings);
        }

        private static void UpdateIdentity<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : Model, new()
        {
            var statement = BuildUpdateIdentityStatement(dbTable, result);
            if (statement != null)
               dbTable.DbSession.Update(statement);
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<TSource> dbTable, InsertTableResult result, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var statement = BuildUpdateIdentityStatement(dbTable, result);
            if (statement != null)
                await dbTable.DbSession.UpdateAsync(statement, cancellationToken);
        }

        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, int ordinal, int? value)
            where TSource : Model, new()
        {
            var oldValue = dataSet.AllowsKeyUpdate(true);
            dataSet._.GetIdentity(false).Column[ordinal] = value;
            dataSet.AllowsKeyUpdate(oldValue);
        }
    }
}
