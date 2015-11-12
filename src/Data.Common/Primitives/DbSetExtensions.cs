
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class DbSetExtensions
    {
        public static DbFromClause GetFromClause<T>(this DbSet<T> dbSet)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            return dbSet.FromClause;
        }
    }
}
