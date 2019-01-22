using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    internal static class DbTableDelete<T>
        where T : class, IModelReference, new()
    {
        public static async Task<int> ExecuteAsync(DbTable<T> from, Func<T, _Boolean> where, CancellationToken ct)
        {
            var statement = from.BuildDeleteStatement(where);
            return from.UpdateOrigin(null, await from.DbSession.DeleteAsync(statement, ct));
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> from, DbSet<TSource> source, IReadOnlyList<ColumnMapping> join, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statement = from.BuildDeleteStatement(source, join);
            return from.UpdateOrigin(null, await from.DbSession.DeleteAsync(statement, ct));
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> from, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> join, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statement = from.BuildDeleteScalarStatement(source, rowIndex, join);
            return from.UpdateOrigin<TSource>(null, await from.DbSession.DeleteAsync(statement, ct) > 0) ? 1 : 0;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> from, DataSet<TSource> source, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (source.Count == 0)
                return 0;
            return await from.DbSession.DeleteAsync(source, from, joinTo, ct);
        }
    }
}
