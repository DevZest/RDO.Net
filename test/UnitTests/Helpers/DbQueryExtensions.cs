﻿using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Helpers
{
    internal static class DbQueryExtensions
    {
        internal static void Verify<T>(this DbQuery<T> dbQuery, string expectedSql)
            where T : Model, new()
        {
            Assert.AreEqual(expectedSql, dbQuery.ToString());
        }

        internal static void MockSequentialKeyTempTable<T>(this DbQuery<T> dbQuery)
            where T : class, IModelReference, new()
        {
            var sequentialKey = new SequentialKey(dbQuery.Model);
            var dbSession = dbQuery.DbSession;
            var tempTableName = dbSession.AssignTempTableName(sequentialKey);
            var queryStatement = dbQuery.QueryStatement;
            queryStatement.SequentialKeyTempTable = DbTable<SequentialKey>.CreateTemp(sequentialKey, dbQuery.DbSession, tempTableName);
            queryStatement.SequentialKeyTempTable.InitialRowCount = 1;  // this value (zero or non-zero) determines whether child query should be created.
        }

        internal static SqlCommand[] GetCreateSequentialKeyTempTableCommands<T>(this DbQuery<T> dbQuery)
            where T : Model, new()
        {
            var sqlSession = (SqlSession)dbQuery.DbSession;
            var tempTableName = "#sys_sequential_" + typeof(T).Name;

            var result = new SqlCommand[2];

            var select = dbQuery.QueryStatement;
            var sequentialKey = new SequentialKey(select.Model);
            var query = select.GetSequentialKeySelectStatement(sequentialKey);
            var tempTable = DbTable<KeyOutput>.MockTemp(sequentialKey, sqlSession, tempTableName);
            result[0] = sqlSession.GetCreateTableCommand(tempTable._, true);
            result[1] = sqlSession.GetInsertCommand(query.BuildToTempTableStatement());
            return result;
        }
    }
}
