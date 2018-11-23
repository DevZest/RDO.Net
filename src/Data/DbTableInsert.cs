using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbTableInsert<T> : Executable<int>
        where T : class, IModelReference, new()
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
            public InsertTableResult(int rowCount, IDbTable identityMappings)
            {
                RowCount = rowCount;
                IdentityMappings = identityMappings;
            }

            public readonly int RowCount;

            public readonly IDbTable IdentityMappings;
        }

        private async Task<InsertTableResult> InsertTableAsync<TSource>(DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(source.Model, updateIdentity, ct);
            var rowCount = await DbSession.InsertAsync(source, Into, columnMapper, joinTo, identityMappings, ct);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<InsertTableResult> InsertDataSetAsync<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(source.Model, updateIdentity, ct);
            var rowCount = await DbSession.InsertAsync(source, Into, columnMapper, joinTo, identityMappings, ct);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private async Task<IDbTable> CreateIdentityMappingsAsync(Model model, bool updateIdentity, CancellationToken ct)
        {
            if (!updateIdentity)
                return null;

            var identity = model.GetIdentity(false);
            if (identity == null)
                return null;

            var column = identity.Column;
            if (column is _Int32)
                return await DbSession.CreateTempTableAsync<Int32IdentityMapping>(ct);
            else if (column is _Int64)
                return await DbSession.CreateTempTableAsync<Int64IdentityMapping>(ct);
            else if (column is _Int16)
                return await DbSession.CreateTempTableAsync<Int16IdentityMapping>(ct);
            else
                return null;
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<TSource> dbTable, InsertTableResult result, CancellationToken cancellationToken)
            where TSource : class, IModelReference, new()
        {
            var statements = BuildUpdateIdentityStatement(dbTable, result);
            if (statements != null)
            {
                foreach (var statement in statements)
                    await dbTable.DbSession.UpdateAsync(statement, cancellationToken);
            }
        }

        private async Task UpdateIdentityAsync<TSource>(DataSet<TSource> dataSet, InsertTableResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityOutput = result.IdentityMappings;
            var identityColumn = dataSet.Model.Columns[Into.Model.GetIdentity(false).Column.Ordinal];
            if (identityOutput is DbTable<Int32IdentityMapping> int32IdentityOutput)
                await UpdateIdentityAsnc(dataSet, (_Int32)identityColumn, int32IdentityOutput, ct);
            else if (identityOutput is DbTable<Int64IdentityMapping> int64IdentityOutput)
                await UpdateIdentityAsnc(dataSet, (_Int64)identityColumn, int64IdentityOutput, ct);
            else if (identityOutput is DbTable<Int16IdentityMapping> int16IdentityOutput)
                await UpdateIdentityAsnc(dataSet, (_Int16)identityColumn, int16IdentityOutput, ct);
            else
                Debug.Fail("identityOutput must be a table of Int32IdentityMapping, Int64IdentityMapping or Int16IdentityMapping.");
        }

        private async Task UpdateIdentityAsnc(DataSet dataSet, _Int32 identityColumn, DbTable<Int32IdentityMapping> identityOutput, CancellationToken ct)
        {
            using (var reader = await DbSession.ExecuteDbReaderAsync(identityOutput, ct))
            {
                int ordinal = 0;
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityOutput._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private async Task UpdateIdentityAsnc(DataSet dataSet, _Int64 identityColumn, DbTable<Int64IdentityMapping> identityOutput, CancellationToken ct)
        {
            using (var reader = await DbSession.ExecuteDbReaderAsync(identityOutput, ct))
            {
                int ordinal = 0;
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityOutput._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private async Task UpdateIdentityAsnc(DataSet dataSet, _Int16 identityColumn, DbTable<Int16IdentityMapping> identityOutput, CancellationToken ct)
        {
            using (var reader = await DbSession.ExecuteDbReaderAsync(identityOutput, ct))
            {
                int ordinal = 0;
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityOutput._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private static IList<DbSelectStatement> BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, InsertTableResult result)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = result.IdentityMappings;
            if (identityMappings == null || result.RowCount == 0)
                return null;

            return dbTable.BuildUpdateIdentityStatement(identityMappings);
        }

        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, DataRow dataRow, long? value)
            where TSource : class, IModelReference, new()
        {
            dataRow.IsPrimaryKeySealed = false;
            var identityColumn = dataSet._.Model.GetIdentity(false).Column;
            if (identityColumn is _Int32 int32Column)
                int32Column[dataRow] = (int?)value;
            else if (identityColumn is _Int64 int64Column)
                int64Column[dataRow] = value;
            else if (identityColumn is _Int16 int16Column)
                int16Column[dataRow] = (short?)value;
            else
                Debug.Fail("Identity column must be _Int32, _Int64 or _Int16.");
            dataRow.IsPrimaryKeySealed = true;
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DbQuery<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IModelReference, new()
        {
            return new InsertFromDbQuery<TSource>(into, source, columnMappings, join);
        }


        private sealed class InsertFromDbQuery<TSource> : DbTableInsert<T>
            where TSource : class, IModelReference, new()
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

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = Into.BuildInsertStatement(_source, _columnMappings, _join);
                return Into.UpdateOrigin(_source, await DbSession.InsertAsync(statement, ct));
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity)
            where TSource : class, IModelReference, new()
        {
            return new InsertFromDbTable<TSource>(into, source, columnMapper, joinTo, updateIdentity);
        }

        private sealed class InsertFromDbTable<TSource> : DbTableInsert<T>
            where TSource : class, IModelReference, new()
        {
            public InsertFromDbTable(DbTable<T> into, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity)
                : base(into)
            {
                _source = source;
                _columnMapper = columnMapper;
                _joinTo = joinTo;
                _updateIdentity = updateIdentity;
            }

            private readonly DbTable<TSource> _source;
            private readonly Action<ColumnMapper, TSource, T> _columnMapper;
            private readonly CandidateKey _joinTo;
            private readonly bool _updateIdentity;

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var result = await InsertTableAsync(_source, _columnMapper, _joinTo, _updateIdentity, ct);
                await UpdateIdentityAsync(_source, result, ct);
                return Into.UpdateOrigin(_source, result.RowCount);
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DataSet<TSource> source, int rowIndex,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join, bool updateIdentity)
            where TSource : class, IModelReference, new()
        {
            return new InsertFromDataRow<TSource>(into, source, rowIndex, columnMappings, join, updateIdentity);
        }

        private sealed class InsertFromDataRow<TSource> : DbTableInsert<T>
            where TSource : class, IModelReference, new()
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

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildInsertStatement();
                var result = await DbSession.InsertScalarAsync(statement, _updateIdentity, ct);
                if (_updateIdentity)
                    UpdateIdentity(_source, _source[_rowIndex], result.IdentityValue);
                return Into.UpdateOrigin(_source, result.Success) ? 1 : 0;
            }
        }

        internal static DbTableInsert<T> Create<TSource>(DbTable<T> into, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity)
            where TSource : class, IModelReference, new()
        {
            return new InsertFromDataSet<TSource>(into, source, columnMapper, joinTo, updateIdentity);
        }

        private sealed class InsertFromDataSet<TSource> : DbTableInsert<T>
            where TSource : class, IModelReference, new()
        {
            public InsertFromDataSet(DbTable<T> into, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity)
                : base(into)
            {
                _source = source;
                _columnMapper = columnMapper;
                _joinTo = joinTo;
                _updateIdentity = updateIdentity;
            }

            private readonly DataSet<TSource> _source;
            private readonly Action<ColumnMapper, TSource, T> _columnMapper;
            private readonly CandidateKey _joinTo;
            private readonly bool _updateIdentity;

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                if (_source.Count == 0)
                    return 0;

                var result = await InsertDataSetAsync(_source, _columnMapper, _joinTo, _updateIdentity, ct);
                await UpdateIdentityAsync(_source, result, ct);
                return Into.UpdateOrigin(_source, result.RowCount);
            }
        }
    }
}
