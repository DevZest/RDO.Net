namespace DevZest.Data.Primitives
{
    public static class DbQueryExtensions
    {
        public static DbQueryStatement GetQueryStatement<T>(this DbQuery<T> dbQuery)
            where T : class, IModelReference, new()
        {
            return dbQuery.QueryStatement;
        }

        public static DbQueryStatement GetSequentialQueryStatement<T>(this DbQuery<T> dbQuery)
            where T : class, IModelReference, new()
        {
            return dbQuery.SequentialQueryStatement;
        }
    }
}
