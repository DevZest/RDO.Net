using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DevZest.Data.MySql.Helpers
{
    internal static class DbSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this DbSession dbSession, T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            return dbSession.CreateTempTableInstance(fromModel, initializer);
        }

        internal static DbTable<T> MockTempTable<T>(this MySqlSession mySqlSession, IList<MySqlCommand> commands, T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            var result = mySqlSession.MockTempTable<T>(fromModel, initializer);
            commands.Add(mySqlSession.GetCreateTableCommand(result.Model, true));
            return result;
        }
    }
}
