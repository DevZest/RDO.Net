
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public static class DbSessionExtensions
    {
        public static DbTable<T> CreateTempTable<T>(this DbSession dbSession, Action<T> initializer, bool addRowId)
            where T : Model, new()
        {
            return dbSession.NewTempTable(initializer, addRowId);
        }

        public static Task<DbTable<T>> CreateTempTableAsync<T>(this DbSession dbSession, Action<T> initializer, bool addRowId, CancellationToken cancellationToken)
            where T : Model, new()
        {
            return dbSession.NewTempTableAsync(initializer, addRowId, cancellationToken);
        }
    }
}
