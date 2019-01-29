using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql.Helpers
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

        internal static MySqlCommand[] GetCreateSequentialKeyTempTableCommands<T>(this DbQuery<T> dbQuery)
            where T : Model, new()
        {
            var mySqlSession = (MySqlSession)dbQuery.DbSession;
            var tempTableName = "#sys_sequential_" + typeof(T).Name;

            var result = new MySqlCommand[2];

            var select = dbQuery.QueryStatement;
            var sequentialKey = new SequentialKey(select.Model);
            var query = select.GetSequentialKeySelectStatement(sequentialKey);
            var tempTable = DbTable<KeyOutput>.MockTemp(sequentialKey, mySqlSession, tempTableName);
            result[0] = mySqlSession.InternalGetCreateTableCommand(tempTable._, true);
            result[1] = mySqlSession.GetInsertCommand(query.BuildToTempTableStatement(), false);
            return result;
        }
    }
}
