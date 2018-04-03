using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbTableInsert<T> : Executable<int>
        where T : Model, new()
    {
        protected DbTableInsert(DbTable<T> into)
        {
            Debug.Assert(into != null);
            _into = into;
        }

        private readonly DbTable<T> _into;
        protected DbTable<T> Into
        {
            get { return _into; }
        }

        protected DbSession DbSession
        {
            get { return Into.DbSession; }
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

        private InsertTableResult InsertTable<TSource>(DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? DbSession.CreateTempTable<IdentityMapping>() : null;
            var rowCount = DbSession.Insert(source, Into, columnMapper, joinTo, identityMappings);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> InsertTableAsync<TSource>(DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Action<IdentityMapping> initializer = null;
            var identityMappings = updateIdentity ? await DbSession.CreateTempTableAsync<IdentityMapping>(null, initializer, cancellationToken) : null;
            var rowCount = await DbSession.InsertAsync(source, Into, columnMapper, joinTo, identityMappings, cancellationToken);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private InsertTableResult InsertDataSet<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
            where TSource : Model, new()
        {
            var identityMappings = updateIdentity ? DbSession.CreateTempTable<IdentityMapping>() : null;
            var rowCount = DbSession.Insert(source, Into, columnMapper, joinTo, identityMappings);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> InsertDataSetAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            Action<IdentityMapping> initializer = null;
            var identityMappings = updateIdentity ? await DbSession.CreateTempTableAsync<IdentityMapping>(null, initializer, cancellationToken) : null;
            var rowCount = await DbSession.InsertAsync(source, Into, columnMapper, joinTo, identityMappings, cancellationToken);
            return new InsertTableResult(rowCount, identityMappings);
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

        private void UpdateIdentity<TSource>(DataSet<TSource> dataSet, InsertTableResult result)
            where TSource : Model, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityMappings = result.IdentityMappings;
            var identityColumn = (_Int32)dataSet._.Columns[Into._.GetIdentity(false).Column.Ordinal];
            using (var reader = DbSession.ExecuteDbReader(identityMappings))
            {
                int ordinal = 0;
                while (reader.Read())
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityMappings._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private async Task UpdateIdentityAsync<TSource>(DataSet<TSource> dataSet, InsertTableResult result, CancellationToken cancellationToken)
            where TSource : Model, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityOutput = result.IdentityMappings;
            var identityColumn = (_Int32)dataSet._.Columns[Into._.GetIdentity(false).Column.Ordinal];
            using (var reader = await DbSession.ExecuteDbReaderAsync(identityOutput, cancellationToken))
            {
                int ordinal = 0;
                while (await reader.ReadAsync(cancellationToken))
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityOutput._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private static IList<DbSelectStatement> BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : Model, new()
        {
            var identityMappings = result.IdentityMappings;
            if (identityMappings == null || result.RowCount == 0)
                return null;

            return dbTable.BuildUpdateIdentityStatement(identityMappings);
        }

        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, DataRow dataRow, int? value)
            where TSource : Model, new()
        {
            dataRow.IsPrimaryKeySealed = false;
            dataSet._.GetIdentity(false).Column[dataRow] = value;
            dataRow.IsPrimaryKeySealed = true;
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DbQuery<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            return new InsertFromDbQuery<TSource>(into, source, columnMappings, join);
        }


        private sealed class InsertFromDbQuery<TSource> : DbTableInsert<T>
            where TSource : Model, new()
        {
            public InsertFromDbQuery(DbTable<T> into, DbQuery<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
                : base(into)
            {
                _source = source;
                _columnMappings = columnMappings;
                _join = join;
            }

            private readonly DbQuery<TSource> _source;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;
            private readonly IReadOnlyList<ColumnMapping> _join;

            private DbSelectStatement BuildInsertStatement()
            {
                return Into.BuildInsertStatement(_source, _columnMappings, _join);
            }

            protected override int PerformExecute()
            {
                var statement = BuildInsertStatement();
                return Into.UpdateOrigin(_source, DbSession.Insert(statement));
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = Into.BuildInsertStatement(_source, _columnMappings, _join);
                return Into.UpdateOrigin(_source, await DbSession.InsertAsync(statement, ct));
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
            where TSource : Model, new()
        {
            return new InsertFromDbTable<TSource>(into, source, columnMapper, joinTo, updateIdentity);
        }

        private sealed class InsertFromDbTable<TSource> : DbTableInsert<T>
            where TSource : Model, new()
        {
            public InsertFromDbTable(DbTable<T> into, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
                : base(into)
            {
                _source = source;
                _columnMapper = columnMapper;
                _joinTo = joinTo;
                _updateIdentity = updateIdentity;
            }

            private readonly DbTable<TSource> _source;
            private readonly Action<ColumnMapper, TSource, T> _columnMapper;
            private readonly PrimaryKey _joinTo;
            private readonly bool _updateIdentity;

            protected override int PerformExecute()
            {
                var result = InsertTable(_source, _columnMapper, _joinTo, _updateIdentity);
                UpdateIdentity(_source, result);
                return Into.UpdateOrigin(_source, result.RowCount);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var result = await InsertTableAsync(_source, _columnMapper, _joinTo, _updateIdentity, ct);
                await UpdateIdentityAsync(_source, result, ct);
                return Into.UpdateOrigin(_source, result.RowCount);
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DataSet<TSource> source, int rowIndex,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join, bool updateIdentity)
            where TSource : Model, new()
        {
            return new InsertFromDataRow<TSource>(into, source, rowIndex, columnMappings, join, updateIdentity);
        }

        private sealed class InsertFromDataRow<TSource> : DbTableInsert<T>
            where TSource : Model, new()
        {
            public InsertFromDataRow(DbTable<T> into, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join, bool updateIdentity)
                : base(into)
            {
                _source = source;
                _rowIndex = rowIndex;
                _columnMappings = columnMappings;
                _join = join;
                _updateIdentity = updateIdentity;
            }

            private readonly DataSet<TSource> _source;
            private readonly int _rowIndex;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;
            private readonly IReadOnlyList<ColumnMapping> _join;
            private readonly bool _updateIdentity;

            private DbSelectStatement BuildInsertStatement()
            {
                return Into.BuildInsertScalarStatement(_source, _rowIndex, _columnMappings, _join);
            }

            protected override int PerformExecute()
            {
                var statement = BuildInsertStatement();
                var result = DbSession.InsertScalar(statement, _updateIdentity);
                if (_updateIdentity)
                    UpdateIdentity(_source, _source[_rowIndex], result.IdentityValue);
                return Into.UpdateOrigin(_source, result.Success) ? 1 : 0;
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildInsertStatement();
                var result = await DbSession.InsertScalarAsync(statement, _updateIdentity, ct);
                if (_updateIdentity)
                    UpdateIdentity(_source, _source[_rowIndex], result.IdentityValue);
                return Into.UpdateOrigin(_source, result.Success) ? 1 : 0;
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
            where TSource : Model, new()
        {
            return new InsertFromDataSet<TSource>(into, source, columnMapper, joinTo, updateIdentity);
        }

        private sealed class InsertFromDataSet<TSource> : DbTableInsert<T>
            where TSource : Model, new()
        {
            public InsertFromDataSet(DbTable<T> into, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, PrimaryKey joinTo, bool updateIdentity)
                : base(into)
            {
                _source = source;
                _columnMapper = columnMapper;
                _joinTo = joinTo;
                _updateIdentity = updateIdentity;
            }

            private readonly DataSet<TSource> _source;
            private readonly Action<ColumnMapper, TSource, T> _columnMapper;
            private readonly PrimaryKey _joinTo;
            private readonly bool _updateIdentity;

            protected override int PerformExecute()
            {
                if (_source.Count == 0)
                    return 0;

                var result = InsertDataSet(_source, _columnMapper, _joinTo, _updateIdentity);
                UpdateIdentity(_source, result);
                return Into.UpdateOrigin(_source, result.RowCount);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                if (_source.Count == 0)
                    return 0;

                var result = await InsertDataSetAsync(_source, _columnMapper, _joinTo, _updateIdentity, ct);
                UpdateIdentity(_source, result);
                return Into.UpdateOrigin(_source, result.RowCount);
            }
        }
    }
}
