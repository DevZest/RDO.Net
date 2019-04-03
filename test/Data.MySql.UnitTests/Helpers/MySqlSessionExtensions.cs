using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DevZest.Data.MySql.Helpers
{
    internal static class MySqlSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this MySqlSession mySqlSession, T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            return mySqlSession.CreateTempTableInstance(fromModel, initializer);
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
