
using DevZest.Data.Primitives;

namespace DevZest.Data.Helpers
{
    internal static class DbSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this DbSession dbSession)
            where T : Model, new()
        {
            var model = new T();
            model.AddTempTableIdentity();
            var tempTableName = dbSession.AssignTempTableName(model);
            return DbTable<T>.CreateTemp(model, dbSession, tempTableName);
        }
    }
}
