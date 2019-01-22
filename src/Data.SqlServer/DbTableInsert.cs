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

        private static async Task<InsertTableResult> InsertTableAsync<TSource>(DbTable<T> target, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(target, source.Model, updateIdentity, ct);
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertAsync(source, target, columnMapper, joinTo, identityMappings, ct);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private static async Task<InsertTableResult> InsertDataSetAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var identityMappings = await CreateIdentityMappingsAsync(target, source.Model, updateIdentity, ct);
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertAsync(source, target, columnMapper, joinTo, identityMappings, ct);
            return new InsertTableResult(rowCount, identityMappings);
        }

        private static async Task<IDbTable> CreateIdentityMappingsAsync(DbTable<T> target, Model model, bool updateIdentity, CancellationToken ct)
        {
            if (!updateIdentity)
                return null;

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

        private static async Task UpdateIdentityAsync<TSource>(DbTable<TSource> dbTable, InsertTableResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statements = BuildUpdateIdentityStatement(dbTable, result);
            if (statements != null)
            {
                foreach (var statement in statements)
                    await dbTable.UpdateAsync(statement, ct);
            }
        }

        private static async Task UpdateIdentityAsync<TSource>(DbTable<T> target, DataSet<TSource> dataSet, InsertTableResult result, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (result.IdentityMappings == null || result.RowCount == 0)
                return;

            var identityOutput = result.IdentityMappings;
            var identityColumn = dataSet.Model.GetColumns()[target.Model.GetIdentity(false).Column.Ordinal];
            if (identityOutput is DbTable<Int32IdentityMapping> int32IdentityOutput)
                await UpdateIdentityAsnc(target, dataSet, (_Int32)identityColumn, int32IdentityOutput, ct);
            else if (identityOutput is DbTable<Int64IdentityMapping> int64IdentityOutput)
                await UpdateIdentityAsnc(target, dataSet, (_Int64)identityColumn, int64IdentityOutput, ct);
            else if (identityOutput is DbTable<Int16IdentityMapping> int16IdentityOutput)
                await UpdateIdentityAsnc(target, dataSet, (_Int16)identityColumn, int16IdentityOutput, ct);
            else
                Debug.Fail("identityOutput must be a table of Int32IdentityMapping, Int64IdentityMapping or Int16IdentityMapping.");
        }

        private static async Task UpdateIdentityAsnc(DbTable<T> target, DataSet dataSet, _Int32 identityColumn, DbTable<Int32IdentityMapping> identityOutput, CancellationToken ct)
        {
            var sqlSession = (SqlSession)target.DbSession;
            using (var reader = await sqlSession.ExecuteReaderAsync(identityOutput, ct))
            {
                int ordinal = 0;
                dataSet.Model.SuspendIdentity();
                while (await reader.ReadAsync(ct))
                {
                    var dataRow = dataSet[ordinal++];
                    identityColumn[dataRow] = identityOutput._.NewValue[reader];
                    dataRow.IsPrimaryKeySealed = true;
                }
                dataSet.Model.ResumeIdentity();
            }
        }

        private static async Task UpdateIdentityAsnc(DbTable<T> target, DataSet dataSet, _Int64 identityColumn, DbTable<Int64IdentityMapping> identityOutput, CancellationToken ct)
        {
            var sqlSession = (SqlSession)target.DbSession;
            using (var reader = await sqlSession.ExecuteReaderAsync(identityOutput, ct))
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

        private static async Task UpdateIdentityAsnc(DbTable<T> target, DataSet dataSet, _Int16 identityColumn, DbTable<Int16IdentityMapping> identityOutput, CancellationToken ct)
        {
            var sqlSession = (SqlSession)target.DbSession;
            using (var reader = await sqlSession.ExecuteReaderAsync(identityOutput, ct))
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

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var result = await InsertTableAsync(target, source, columnMapper, joinTo, updateIdentity, ct);
            await UpdateIdentityAsync(source, result, ct);
            return result.RowCount;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo,
            bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (source.Count == 0)
                return 0;

            var result = await InsertDataSetAsync(target, source, columnMapper, joinTo, updateIdentity, ct);
            await UpdateIdentityAsync(target, source, result, ct);
            return result.RowCount;
        }
    }
}
