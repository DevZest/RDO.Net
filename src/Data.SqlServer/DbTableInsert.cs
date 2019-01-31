using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
    internal static class DbTableInsert<T>
        where T : class, IModelReference, new()
    {
        private struct IdentityMappingsInsertResult
        {
            public IdentityMappingsInsertResult(int rowCount, IDbTable identityMappings)
            {
                RowCount = rowCount;
                IdentityMappings = identityMappings;
            }

            public readonly int RowCount;

            public readonly IDbTable IdentityMappings;
        }

        private struct IdentityOutputInsertResult
        {
            public IdentityOutputInsertResult(int rowCount, IDbTable identityOutput)
            {
                RowCount = rowCount;
                IdentityOutput = identityOutput;
            }

            public readonly int RowCount;

            public readonly IDbTable IdentityOutput;
        }

        private static async Task<IdentityMappingsInsertResult> InsertTableWithUpdateIdentityAsync<TSource>(DbTable<T> target, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(target, source.Model, ct);
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertAsync(source, target, columnMapper, joinTo, identityMappings, ct);
            return new IdentityMappingsInsertResult(rowCount, identityMappings);
        }

        private static async Task<IdentityOutputInsertResult> InsertDataSetWithIdentityOutputAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            Debug.Assert(joinTo == null || !updateIdentity);
            var identityOutput = updateIdentity ? await CreateIdentityOutputAsync(target, source.Model, ct) : null;
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertWithIdentityOutputAsync(source, target, columnMapper, joinTo, identityOutput, ct);
            return new IdentityOutputInsertResult(rowCount, identityOutput);
        }

        private static async Task<IDbTable> CreateIdentityOutputAsync(DbTable<T> target, Model model, CancellationToken ct)
        {
            var identity = model.GetIdentity(false);
            if (identity == null)
                return null;

            var column = identity.Column;
            if (column is _Int32)
                return await target.DbSession.CreateTempTableAsync<Int32IdentityOutput>(ct);
            else if (column is _Int64)
                return await target.DbSession.CreateTempTableAsync<Int64IdentityOutput>(ct);
            else if (column is _Int16)
                return await target.DbSession.CreateTempTableAsync<Int16IdentityOutput>(ct);
            else
                return null;
        }

        private static async Task<IdentityMappingsInsertResult> InsertDataSetWithIdentityMappingsAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(target, source.Model, ct);
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertWithIdentityMappingsAsync(source, target, columnMapper, joinTo, identityMappings, ct);
            return new IdentityMappingsInsertResult(rowCount, identityMappings);
        }

        private static async Task<IDbTable> CreateIdentityMappingsAsync(DbTable<T> target, Model model, CancellationToken ct)
        {
            var identity = model.GetIdentity(false);
            if (identity == null)
                return null;

            var column = identity.Column;
            if (column is _Int32)
                return await target.DbSession.CreateTempTableAsync<Int32IdentityMapping>(ct);
            else if (column is _Int64)
                return await target.DbSession.CreateTempTableAsync<Int64IdentityMapping>(ct);
            else if (column is _Int16)
                return await target.DbSession.CreateTempTableAsync<Int16IdentityMapping>(ct);
            else
                return null;
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<TSource> dbTable, IdentityMappingsInsertResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statements = BuildUpdateIdentityStatement(dbTable, result);
            if (statements != null)
            {
                foreach (var statement in statements)
                    await dbTable.UpdateAsync(statement, ct);
            }
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<T> target, DataSet<TSource> dataSet, IdentityOutputInsertResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (result.IdentityOutput == null || result.RowCount == 0)
                return;

            var identityOutput = result.IdentityOutput;
            var identityColumn = dataSet.Model.GetColumns()[target.Model.GetIdentity(false).Column.Ordinal];
            if (identityOutput is DbTable<Int32IdentityOutput> int32IdentityOutput)
                await UpdateFromIdentityOutputAsync(dataSet, (_Int32)identityColumn, int32IdentityOutput, ct);
            else if (identityOutput is DbTable<Int64IdentityOutput> int64IdentityOutput)
                await UpdateFromIdentityOutputAsync(dataSet, (_Int64)identityColumn, int64IdentityOutput, ct);
            else if (identityOutput is DbTable<Int16IdentityOutput> int16IdentityOutput)
                await UpdateFromIdentityOutputAsync(dataSet, (_Int16)identityColumn, int16IdentityOutput, ct);
            else
                Debug.Fail("identityOutput must be a table of Int32IdentityOutput, Int64IdentityOutput or Int16IdentityOutput.");
        }

        private static async Task UpdateFromIdentityOutputAsync<TIdentity, TIdentityOutput>(DataSet dataSet, TIdentity identityColumn, DbTable<TIdentityOutput> identityOutput, CancellationToken ct)
            where TIdentity : Column
            where TIdentityOutput : Model, IIdentityOutput<TIdentity>, new()
        {
            var _ = identityOutput._;
            var sqlSession = (SqlSession)identityOutput.DbSession;
            using (var reader = await sqlSession.ExecuteReaderAsync(identityOutput, ct))
            {
                int ordinal = 0;
                dataSet.Model.SuspendIdentity();
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[ordinal++];
                    _.Update(identityColumn, dataRow, reader);
                    dataRow.IsPrimaryKeySealed = true;
                }
                dataSet.Model.ResumeIdentity();
            }
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<T> target, DataSet<TSource> dataSet, IdentityMappingsInsertResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityMappings = result.IdentityMappings;
            var identityColumn = dataSet.Model.GetColumns()[target.Model.GetIdentity(false).Column.Ordinal];
            if (identityMappings is DbTable<Int32IdentityMapping> int32IdentityOutput)
                await UpdateFromIdentityMappingsAsnc(dataSet, (_Int32)identityColumn, int32IdentityOutput, ct);
            else if (identityMappings is DbTable<Int64IdentityMapping> int64IdentityOutput)
                await UpdateFromIdentityMappingsAsnc(dataSet, (_Int64)identityColumn, int64IdentityOutput, ct);
            else if (identityMappings is DbTable<Int16IdentityMapping> int16IdentityOutput)
                await UpdateFromIdentityMappingsAsnc(dataSet, (_Int16)identityColumn, int16IdentityOutput, ct);
            else
                Debug.Fail("identityOutput must be a table of Int32IdentityMapping, Int64IdentityMapping or Int16IdentityMapping.");
        }

        private static async Task UpdateFromIdentityMappingsAsnc<TIdentity, TIdentityMapping>(DataSet dataSet, TIdentity identityColumn, DbTable<TIdentityMapping> identityMappings, CancellationToken ct)
            where TIdentity : Column
            where TIdentityMapping : Model, IIdentityMapping, IIdentityOutput<TIdentity>, new()
        {
            var _ = identityMappings._;
            var sqlSession = (SqlSession)identityMappings.DbSession;
            using (var reader = await sqlSession.ExecuteReaderAsync(identityMappings, ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[_.OriginalSysRowId[reader].Value];
                    _.Update(identityColumn, dataRow, reader);
                    dataRow.IsPrimaryKeySealed = true;
                }
            }
        }

        private static IList<DbSelectStatement> BuildUpdateIdentityStatement<TSource>(DbTable<TSource> dbTable, IdentityMappingsInsertResult result)
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
            var model = dataSet._.Model;
            model.SuspendIdentity();
            dataRow.IsPrimaryKeySealed = false;
            var identityColumn = model.GetIdentity(false).Column;
            if (identityColumn is _Int32 int32Column)
                int32Column[dataRow] = (int?)value;
            else if (identityColumn is _Int64 int64Column)
                int64Column[dataRow] = value;
            else if (identityColumn is _Int16 int16Column)
                int16Column[dataRow] = (short?)value;
            else
                Debug.Fail("Identity column must be _Int32, _Int64 or _Int16.");
            model.ResumeIdentity();
            dataRow.IsPrimaryKeySealed = true;
        }

        public static async Task<int> ExecuteWithUpdateIdentityAsync<TSource>(DbTable<T> target, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var result = await InsertTableWithUpdateIdentityAsync(target, source, columnMapper, joinTo, ct);
            await UpdateIdentityAsync(source, result, ct);
            return result.RowCount;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo,
            bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (joinTo != null && updateIdentity)
            {
                // We need a result of DbTable<IdentityMapping>
                var result = await InsertDataSetWithIdentityMappingsAsync(target, source, columnMapper, joinTo, ct);
                await UpdateIdentityAsync(target, source, result, ct);
                return result.RowCount;
            }
            else
            {
                // otherwise we only need a result of DbTable<IdentityOutput> if updateIdentity is true.
                var result = await InsertDataSetWithIdentityOutputAsync(target, source, columnMapper, joinTo, updateIdentity, ct);
                await UpdateIdentityAsync(target, source, result, ct);
                return result.RowCount;
            }
        }
    }
}
