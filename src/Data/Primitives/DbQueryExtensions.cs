namespace DevZest.Data.Primitives
{
    public static class DbQueryExtensions
    {
        public static DbQuery<T> MockSequentialKeyTempTable<T>(this DbQuery<T> dbQuery, int initialRowCount = 1)
            where T : class, IModelReference, new()
        {
            // Create DbTable object for SequentialKeyTempTable without actually creating the temp table in the database.
            var select = dbQuery.QueryStatement;
            var sequentialKeyModel = new KeyOutput(select.Model, true);
            var sequentialSelect = select.GetSequentialKeySelectStatement(sequentialKeyModel);
            var tempTableName = dbQuery.DbSession.AssignTempTableName(sequentialKeyModel);
            select.SequentialKeyTempTable = DbTable<KeyOutput>.CreateTemp(sequentialKeyModel, dbQuery.DbSession, tempTableName);
            select.SequentialKeyTempTable.InitialRowCount = initialRowCount;  // this value (zero or non-zero) determines whether child query should be created.

            return dbQuery;
        }

        public static DbQueryStatement GetQueryStatement<T>(this DbQuery<T> dbQuery)
            where T : class, IModelReference, new()
        {
            return dbQuery.QueryStatement;
        }
    }
}
