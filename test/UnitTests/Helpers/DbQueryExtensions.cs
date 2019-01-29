using DevZest.Data.MySql;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

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
            // Create DbTable object for SequentialKeyTempTable without actually creating the temp table in the database.
            dbQuery.GetQueryStatement().MockSequentialKeyTempTable(dbQuery.DbSession);
        }

        internal static MySqlCommand[] GetCreateSequentialKeyTempTableCommands<T>(this DbQuery<T> dbQuery)
            where T : Model, new()
        {
            var mySqlSession = (MySqlSession)dbQuery.DbSession;
            var tempTableName = "#sys_sequential_" + typeof(T).Name;

            var result = new MySqlCommand[2];

            var select = dbQuery.GetQueryStatement();
            var sequentialKey = new SequentialKey(select.Model);
            var query = select.GetSequentialKeySelectStatement(sequentialKey);
            var tempTable = DbTable<KeyOutput>.MockTemp(sequentialKey, mySqlSession, tempTableName);
            result[0] = mySqlSession.InternalGetCreateTableCommand(tempTable._, true);
            result[1] = mySqlSession.GetInsertCommand(query.BuildToTempTableStatement(), false);
            return result;
        }
    }
}
