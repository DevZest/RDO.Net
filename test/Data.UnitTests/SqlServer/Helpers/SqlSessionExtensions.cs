using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DevZest.Data.SqlServer.Helpers
{
    internal static class SqlSessionExtensions
    {
        internal static DbTable<T> MockTempTable<T>(this SqlSession sqlSession, T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            return sqlSession.CreateTempTableInstance(fromModel, initializer);
        }

        internal static DbTable<T> MockTempTable<T>(this SqlSession sqlSession, IList<SqlCommand> commands, T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            var result = sqlSession.MockTempTable<T>(fromModel, initializer);
            commands.Add(sqlSession.GetCreateTableCommand(result.Model, true));
            return result;
        }
    }
}
