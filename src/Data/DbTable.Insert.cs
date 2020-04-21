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
        /// <summary>
        /// Inserts default values into this database table.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync(CancellationToken ct = default(CancellationToken))
        {
            return DbTableInsert<T>.ExecuteAsync(this, Array.Empty<ColumnMapping>(), null, ct);
        }

        /// <summary>
        /// Inserts default values into this database table.
        /// </summary>
        /// <param name="outputIdentity">Delegate to receive newly generated identity value.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync(Action<long?> outputIdentity, CancellationToken ct = default(CancellationToken))
        {
            return DbTableInsert<T>.ExecuteAsync(this, Array.Empty<ColumnMapping>(), outputIdentity, ct);
        }

        /// <summary>
        /// Inserts values into this database table.
        /// </summary>
        /// <param name="columnMapper">Provides column mappings between value and column in this table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync(Action<ColumnMapper, T> columnMapper, CancellationToken ct = default(CancellationToken))
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableInsert<T>.ExecuteAsync(this, columnMappings, null, ct);
        }

        /// <summary>
        /// Inserts values into this database table.
        /// </summary>
        /// <param name="columnMapper">Provides Column mappings between value and column in this table.</param>
        /// <param name="outputIdentity">Delegate to receive newly generated identity value.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync(Action<ColumnMapper, T> columnMapper, Action<long?> outputIdentity, CancellationToken ct = default(CancellationToken))
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableInsert<T>.ExecuteAsync(this, columnMappings, outputIdentity, ct);
        }

        /// <summary>
        /// Inserts into this database table from database recordset.
        /// </summary>
        /// <typeparam name="TSource">Entity type of database recordset.</typeparam>
        /// <param name="source">The source database recordset.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DbSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => m.AutoSelectInsertable(), ct);
        }

        /// <summary>
        /// Inserts into this database table from database recordset.
        /// </summary>
        /// <typeparam name="TSource">Entity type of database recordset.</typeparam>
        /// <param name="source">The source database recordset.</param>
        /// <param name="columnMapper">Provides column mappings between source database recordset and this table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : Model, new()
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
            where TSource : Model, new()
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

        /// <summary>
        /// Inserts into this table from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => m.AutoSelectInsertable(), false, ct);
        }

        /// <summary>
        /// Inserts into this table from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="updateIdentity">Specifies whether source DataSet identity column should be updated with newly generated values.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : class, T, new()
        {
            return InsertAsync(source, (m, s, t) => m.AutoSelectInsertable(), updateIdentity, ct);
        }

        /// <summary>
        /// Inserts into this table from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and this table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : Model, new()
        {
            return InsertAsync(source, columnMapper, false, ct);
        }

        /// <summary>
        /// Inserts into this table from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and this table.</param>
        /// <param name="updateIdentity">Specifies whether source DataSet identity column should be updated with newly generated values.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : Model, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return InsertAsync(source, 0, columnMapper, updateIdentity, ct);

            Verify(columnMapper, nameof(columnMapper));
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.ExecuteAsync(this, source, columnMapper, updateIdentity, ct);
        }

        /// <summary>
        /// Inserts specified DataRow in DataSet into this table.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="ordinal">The ordianl to specify the source DataRow.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, CancellationToken ct = default(CancellationToken))
            where TSource : T, new()
        {
            return InsertAsync(source, ordinal, (m, s, t) => m.AutoSelectInsertable(), false, ct);
        }

        /// <summary>
        /// Inserts specified DataRow in DataSet into this table.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="ordinal">The ordianl to specify the source DataRow.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and this table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMapper, TSource, T> columnMapper, CancellationToken ct = default(CancellationToken))
            where TSource : Model, new()
        {
            return InsertAsync(source, ordinal, columnMapper, false, ct);
        }

        /// <summary>
        /// Inserts specified DataRow in DataSet into this table.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="ordinal">The ordianl to specify the source DataRow.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and this table.</param>
        /// <param name="updateIdentity">Specifies whether source DataSet identity column should be updated with newly generated value.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of records inserted into this database table.</returns>
        public Task<int> InsertAsync<TSource>(DataSet<TSource> source, int ordinal, Action<ColumnMapper, TSource, T> columnMapper, bool updateIdentity, CancellationToken ct = default(CancellationToken))
            where TSource : Model, new()
        {
            Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.ExecuteAsync(this, source, ordinal, columnMappings, updateIdentity, ct);
        }

        internal DbSelectStatement BuildInsertScalarStatement<TSource>(DataSet<TSource> dataSet, int rowOrdinal, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : Model, new()
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
