
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Helpers
{
    internal static class DbSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this DbSession dbSession, Action<T> initializer = null)
            where T : Model, new()
        {
            return dbSession.NewTempTableObject(initializer);
        }
    }
}
