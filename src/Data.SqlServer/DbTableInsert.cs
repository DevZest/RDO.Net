using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
    internal static class DbTableInsert<T>
        where T : class, IEntity, new()
    {
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

        private static async Task<IdentityOutputInsertResult> InsertDataSetWithIdentityOutputAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, bool updateIdentity, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var identityOutputs = updateIdentity ? await CreateIdentityOutputsAsync(target, source.Model, ct) : null;
            var sqlSession = (SqlSession)target.DbSession;
            var rowCount = await sqlSession.InsertAsync(source, target, columnMapper, identityOutputs, ct);
            return new IdentityOutputInsertResult(rowCount, identityOutputs);
        }

        private static async Task<IDbTable> CreateIdentityOutputsAsync(DbTable<T> target, Model model, CancellationToken ct)
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

        private static async Task UpdateIdentityAsync<TSource>(DbTable<T> target, DataSet<TSource> dataSet, IdentityOutputInsertResult result, CancellationToken ct)
            where TSource : class, IEntity, new()
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

        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, DataRow dataRow, long? value)
            where TSource : class, IEntity, new()
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

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper,
            bool updateIdentity, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var result = await InsertDataSetWithIdentityOutputAsync(target, source, columnMapper, updateIdentity, ct);
            await UpdateIdentityAsync(target, source, result, ct);
            return result.RowCount;
        }
    }
}
