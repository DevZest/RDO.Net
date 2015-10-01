using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Helpers
{
    internal static class DbSetExtensions
    {
        internal static SqlCommand[] GetToTempTableCommands<T>(this DbSet<T> dbSet)
            where T : Model, new()
        {
            var sqlSession = (SqlSession)dbSet.DbSession;

            var result = new SqlCommand[2];

            var model = Model.Clone(dbSet._, false);
            var tempTableName = sqlSession.AssignTempTableName(model);
            var builder = dbSet.QueryStatement.MakeQueryBuilder(sqlSession, model, false);
            var insertStatement = builder.BuildQueryStatement(true);
            var tempTable = DbTable<T>.CreateTemp(model, sqlSession, tempTableName);
            result[0] = sqlSession.GetCreateTableCommand(model, tempTableName, true);
            result[1] = sqlSession.GetInsertCommand(insertStatement.BuildToTempTableStatement(tempTable));

            return result;
        }

        internal static DbTable<T> MockTempTable<T>(this DbSet<T> dbSet)
            where T : Model, new()
        {
            var model = Model.Clone(dbSet._, false);
            model.AddTempTableIdentity();
            var tempTableName = dbSet.DbSession.AssignTempTableName(model);
            return DbTable<T>.CreateTemp(model, dbSet.DbSession, tempTableName);
        }
    }
}
