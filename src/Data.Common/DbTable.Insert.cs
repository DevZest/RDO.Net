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
        public int Insert<TSource>(DbQuery<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            return UpdateOrigin(source, DbSession.Insert(BuildInsertStatement(source, columnMappingsBuilder, autoJoin)));
        }

        public async Task<int> InsertAsync<TSource>(DbQuery<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            return UpdateOrigin(source, await DbSession.InsertAsync(BuildInsertStatement(source, columnMappingsBuilder, autoJoin), cancellationToken));
        }

        public Task<int> InsertAsync<TSource>(DbQuery<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
        {
            return InsertAsync(source, columnMappingsBuilder, autoJoin, CancellationToken.None);
        }

        public int Insert<TSource>(DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = InsertTable(source, columnMappingsBuilder, autoJoin, updateIdentity);
            UpdateIdentity(source, result);
            return UpdateOrigin(source, result.RowCount);
        }

        public async Task<int> InsertAsync<TSource>(DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = await InsertTableAsync(source, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken);
            await UpdateIdentityAsync(source, result, cancellationToken);
            return UpdateOrigin(source, result.RowCount);
        }

        public Task<int> InsertAsync<TSource>(DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            return InsertAsync(source, columnMappingsBuilder, autoJoin, updateIdentity, CancellationToken.None);
        }

        internal void VerifyUpdateIdentity(bool updateIdentity, string paramName)
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

        private InsertTableResult InsertTable<TSource>(DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? DbSession.CreateTempTable<IdentityMapping>() : null;
            var rowCount = DbSession.Insert(source, this, columnMappingsBuilder, autoJoin, identityMappings);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> InsertTableAsync<TSource>(DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Action<IdentityMapping> initializer = null;
            var identityMappings = updateIdentity ? await DbSession.CreateTempTableAsync<IdentityMapping>(initializer, cancellationToken) : null;
            var rowCount = await DbSession.InsertAsync(source, this, columnMappingsBuilder, autoJoin, identityMappings, cancellationToken);
            return new InsertTableResult(rowCount, identityMappings);
        }

        internal DbSelectStatement BuildInsertStatement<TSource>(DbSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
        {
            var sourceModel = source._;
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : _.BuildColumnMappings(sourceModel, columnMappingsBuilder);
            var keyMappings = autoJoin ? GetKeyMappings(sourceModel) : null;
            return source.QueryStatement.BuildInsertStatement(Model, columnMappings, keyMappings, ShouldJoinParent(source));
        }

        private bool ShouldJoinParent(DataSource sourceData)
        {
            Debug.Assert(sourceData != null);

            var parentModel = Model.ParentModel;
            if (parentModel == null)
                return false;

            sourceData = sourceData.UltimateOriginalDataSource;
            if (sourceData == null)
                return true;
            var sourceParentModel = sourceData.Model.ParentModel;
            if (sourceParentModel == null)
                return true;
            var parentDataSource = sourceParentModel.DataSource;
            if (parentDataSource == null)
                return true;

            return parentModel.DataSource.UltimateOriginalDataSource != parentDataSource.UltimateOriginalDataSource;
        }

        public int Insert<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return UpdateOrigin(source, InsertDataSet(source, columnMappingsBuilder, autoJoin, updateIdentity));
        }

        public async Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return UpdateOrigin(source, await InsertDataSetAsync(source, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken));
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
        {
            return InsertAsync(source, columnMappingsBuilder, autoJoin, updateIdentity, CancellationToken.None);
        }

        private int InsertDataSet<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return Insert(source, 0, columnMappingsBuilder, autoJoin, updateIdentity) ? 1 : 0;

            var result = DoInsertDataSet(source, columnMappingsBuilder, autoJoin, updateIdentity);
            UpdateIdentity(source, result);
            return result.RowCount;
        }

        private InsertTableResult DoInsertDataSet<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? DbSession.CreateTempTable<IdentityMapping>() : null;
            var rowCount = DbSession.Insert(source, this, columnMappingsBuilder, autoJoin, identityMappings);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> DoInsertDataSetAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Action<IdentityMapping> initializer = null;
            var identityMappings = updateIdentity ? await DbSession.CreateTempTableAsync<IdentityMapping>(initializer, cancellationToken) : null;
            var rowCount = await DbSession.InsertAsync(source, this, columnMappingsBuilder, autoJoin, identityMappings, cancellationToken);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<int> InsertDataSetAsync<TSource>(DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            if (source.Count == 0)
                return 0;

            if (source.Count == 1)
                return await InsertAsync(source, 0, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken) ? 1 : 0;

            var result = await DoInsertDataSetAsync(source, columnMappingsBuilder, autoJoin, updateIdentity, cancellationToken);
            UpdateIdentity(source, result);
            return result.RowCount;
        }

        public bool Insert<TSource>(DataSet<TSource> source, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var statement = BuildInsertScalarStatement(source, rowOrdinal, columnMappingsBuilder, autoJoin);
            var result = DbSession.InsertScalar(statement, updateIdentity);
            if (updateIdentity)
                UpdateIdentity(source, rowOrdinal, result.IdentityValue);
            return UpdateOrigin(source, result.Success);
        }

        public async Task<bool> InsertAsync<TSource>(DataSet<TSource> source, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var statement = BuildInsertScalarStatement(source, rowOrdinal, columnMappingsBuilder, autoJoin);
            var result = await DbSession.InsertScalarAsync(statement, updateIdentity, cancellationToken);
            if (updateIdentity)
                UpdateIdentity(source, rowOrdinal, result.IdentityValue);
            return UpdateOrigin(source, result.Success);
        }

        internal DbSelectStatement BuildInsertScalarStatement<TSource>(DataSet<TSource> dataSet, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
        {
            var sourceModel = dataSet._;
            var columnMappings = columnMappingsBuilder == null ? GetColumnMappings(sourceModel) : _.BuildColumnMappings(sourceModel, columnMappingsBuilder);
            var keyMappings = autoJoin ? GetKeyMappings(sourceModel) : null;
            var parentMappings = ShouldJoinParent(dataSet) ? columnMappings.GetParentRelationship(this) : null;

            var paramManager = new ScalarParamManager(dataSet[rowOrdinal]);
            var select = GetScalarMapping(paramManager, columnMappings);
            IDbTable parentTable = null;
            if (parentMappings != null)
            {
                parentTable = (IDbTable)Model.ParentModel.DataSource;
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

            return new DbSelectStatement(Model, select, from, where, null, -1, -1);
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

        private static IList<DbSelectStatement> BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : Model, new()
        {
            var identityMappings = result.IdentityMappings;
            if (identityMappings == null || result.RowCount == 0)
                return null;

            return dbTable.BuildUpdateIdentityStatement(identityMappings);
        }

        private static void UpdateIdentity<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : Model, new()
        {
            var statements = BuildUpdateIdentityStatement(dbTable, result);
            if (statements != null)
            {
                foreach (var statement in statements)
                    dbTable.DbSession.Update(statement);
            }
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<TSource> dbTable, InsertTableResult result, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            var statements = BuildUpdateIdentityStatement(dbTable, result);
            if (statements != null)
            {
                foreach (var statement in statements)
                    await dbTable.DbSession.UpdateAsync(statement, cancellationToken);
            }
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
