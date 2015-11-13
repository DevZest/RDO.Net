
using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DevZest.Data.Helpers
{
    internal static class DbSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this DbSession dbSession, Action<T> initializer = null)
            where T : Model, new()
        {
            return dbSession.NewTempTableObject(initializer);
        }

        internal static DbTable<T> MockTempTable<T>(this SqlSession sqlSession, IList<SqlCommand> commands)
            where T : Model, new()
        {
            var result = sqlSession.MockTempTable<T>();
            commands.Add(sqlSession.GetCreateTableCommand(result.Model, result.Name, true));
            return result;
        }
    }
}
