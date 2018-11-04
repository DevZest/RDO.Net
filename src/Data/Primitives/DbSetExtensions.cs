namespace DevZest.Data.Primitives
{
    public static class DbSetExtensions
    {
        public static DbFromClause GetFromClause(this IDbSet dbSet)
        {
            dbSet.VerifyNotNull(nameof(dbSet));
            return dbSet.FromClause;
        }
    }
}
