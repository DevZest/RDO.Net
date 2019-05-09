using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    internal static class DbTableInsert<T>
        where T : class, IEntity, new()
    {
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

        public static async Task<int> ExecuteAsync(DbTable<T> target, IReadOnlyList<ColumnMapping> columnMappings, CancellationToken ct)
        {
            var statement = target.BuildInsertStatement(columnMappings);
            var result = await target.DbSession.InsertScalarAsync(statement, false, ct);
            return target.UpdateOrigin(null, result.Success ? 1 : 0);
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var statement = target.BuildInsertStatement(source, columnMappings);
            return target.UpdateOrigin(source, await target.DbSession.InsertAsync(statement, ct));
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, int rowIndex,
            IReadOnlyList<ColumnMapping> columnMappings, bool updateIdentity, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var statement = target.BuildInsertScalarStatement(source, rowIndex, columnMappings);
            var result = await target.DbSession.InsertScalarAsync(statement, updateIdentity, ct);
            if (updateIdentity)
                UpdateIdentity(source, source[rowIndex], result.IdentityValue);
            return target.UpdateOrigin(source, result.Success) ? 1 : 0;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper,
            bool updateIdentity, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            if (source.Count == 0)
                return 0;

            var result = await target.DbSession.InsertAsync(source, target, columnMapper, updateIdentity, ct);
            return target.UpdateOrigin(source, result);
        }
    }
}
