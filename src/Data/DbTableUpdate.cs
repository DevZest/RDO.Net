using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    internal static class DbTableUpdate<T>
        where T : class, IEntity, new()
    {
        public static async Task<int> ExecuteAsync(DbTable<T> target, IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where, CancellationToken ct)
        {
            var statement = target.BuildUpdateStatement(columnMappings, where);
            return target.UpdateOrigin(null, await target.DbSession.UpdateAsync(statement, ct));
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join,
            CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var statement = target.BuildUpdateStatement(source, columnMappings, join);
            return target.UpdateOrigin(null, await target.DbSession.UpdateAsync(statement, ct));
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings,
            IReadOnlyList<ColumnMapping> join, CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            var statement = target.BuildUpdateScalarStatement(source, rowIndex, columnMappings, join);
            return target.UpdateOrigin<TSource>(null, await target.DbSession.UpdateAsync(statement, ct) > 0) ? 1 : 0;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo,
            CancellationToken ct)
            where TSource : class, IEntity, new()
        {
            if (source.Count == 0)
                return 0;
            return await target.DbSession.UpdateAsync(source, target, columnMapper, joinTo, ct);
        }
    }
}
