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
        public Task<int> InsertAsync(CancellationToken ct = default(CancellationToken))
        {
            return DbTableInsert<T>.ExecuteAsync(this, Array.Empty<ColumnMapping>(), null, ct);
        }

        public Task<int> InsertAsync(Action<long?> outputIdentity, CancellationToken ct = default(CancellationToken))
        {
            return DbTableInsert<T>.ExecuteAsync(this, Array.Empty<ColumnMapping>(), outputIdentity, ct);
        }

        public Task<int> InsertAsync(Action<ColumnMapper, T> columnMapper, CancellationToken ct = default(CancellationToken))
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableInsert<T>.ExecuteAsync(this, columnMappings, null, ct);
        }

        public Task<int> InsertAsync(Action<ColumnMapper, T> columnMapper, Action<long?> outputIdentity, CancellationToken ct = default(CancellationToken))
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableInsert<T>.ExecuteAsync(this, columnMappings, outputIdentity, ct);
        }

        public Task<int> InsertAsync<TSource>(DbSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), ct);
        }

        public Task<int> InsertAsync<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            return DbTableInsert<T>.ExecuteAsync(this, source, columnMappings, ct);
        }

        internal void VerifyUpdateIdentity(bool updateIdentity, string paramName)
        {
            if (!updateIdentity)
                return;

            if (Kind == DataSourceKind.DbTempTable || Model.GetIdentity(false) == null)
                throw new ArgumentException(DiagnosticMessages.DbTable_VerifyUpdateIdentity, paramName);
        }

        internal DbSelectStatement BuildInsertStatement(IReadOnlyList<ColumnMapping> columnMappings)
        {
            Debug.Assert(columnMappings != null);
            return new DbSelectStatement(Model, columnMappings, null, null, null, -1, -1);
        }

        internal DbSelectStatement BuildInsertStatement<TSource>(DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : class, IEntity, new()
        {
            var sourceModel = source._;
            return source.QueryStatement.BuildInsertStatement(Model, columnMappings, ShouldJoinParent(source));
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

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), false, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), updateIdentity, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            return InsertAsync(source, columnMapper, false, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return InsertAsync(source, 0, columnMapper, updateIdentity, ct);

            Verify(columnMapper, nameof(columnMapper));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.ExecuteAsync(this, source, columnMapper, updateIdentity, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, ordinal, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), false, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            return InsertAsync(source, ordinal, columnMapper, false, ct);
        }

        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMapper, TSource, T> columnMapper, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : class, IEntity, new()
        {
            Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.ExecuteAsync(this, source, ordinal, columnMappings, updateIdentity, ct);
        }

        internal DbSelectStatement BuildInsertScalarStatement<TSource>(DataSet<TSource> dataSet, int rowOrdinal, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : class, IEntity, new()
        {
            var sourceModel = dataSet._;
            var parentMappings = ShouldJoinParent(dataSet) ? this.Model.GetParentRelationship(columnMappings) : null;

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

            DbFromClause from = GetScalarDataSource(paramManager, parentMappings);
            DbExpression where = null;
            if (from != null)
            {
                if (parentMappings != null)
                    from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentTable.FromClause, parentMappings);
            }

            return new DbSelectStatement(Model, select, from, where, null, -1, -1);
        }
    }
}
