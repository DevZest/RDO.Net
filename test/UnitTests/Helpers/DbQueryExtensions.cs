//using DevZest.Data.Primitives;
//using DevZest.Data.SqlServer;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Data.SqlClient;

//namespace DevZest.Data.Helpers
//{
//    internal static class DbQueryExtensions
//    {
//        internal static void Verify<T>(this DbQuery<T> dbQuery, string expectedSql)
//            where T : Model, new()
//        {
//            Assert.AreEqual(expectedSql, dbQuery.ToString());
//        }

//        internal static DbQuery<T> MockSequentialKeyTempTable<T>(this DbQuery<T> dbQuery, int initialRowCount = 1)
//            where T : Model, new()
//        {
//            // Create DbTable object for SequentialKeyTempTable without actually creating the temp table in the database.
//            var select = dbQuery.QueryStatement;
//            var sequentialKeyModel = select.Model.CreateSequentialKey();
//            var sequentialSelect = select.GetSequentialKeySelectStatement(sequentialKeyModel);
//            var tempTableName = dbQuery.DbSession.AssignTempTableName(sequentialKeyModel);
//            select.SequentialKeyTempTable = DbTable<KeyOutput>.CreateTemp(sequentialKeyModel, dbQuery.DbSession, tempTableName);
//            select.SequentialKeyTempTable.InitialRowCount = initialRowCount;  // this value (zero or non-zero) determines whether child query should be created.

//            return dbQuery;
//        }

//        internal static SqlCommand[] GetCreateSequentialKeyTempTableCommands<T>(this DbQuery<T> dbQuery)
//            where T : Model, new()
//        {
//            var sqlSession = (SqlSession)dbQuery.DbSession;
//            var tempTableName = "#sys_sequential_" + typeof(T).Name;

//            var result = new SqlCommand[2];

//            var select = dbQuery.QueryStatement;
//            var query = select.GetSequentialKeySelectStatement(select.Model.CreateSequentialKey());
//            var model = (KeyOutput)query.Model;
//            var tempTable = DbTable<KeyOutput>.CreateTemp(model, sqlSession, tempTableName);
//            result[0] = sqlSession.GetCreateTableCommand(tempTable._, true);
//            result[1] = sqlSession.GetInsertCommand(query.BuildToTempTableStatement());
//            return result;
//        }
//    }
//}
