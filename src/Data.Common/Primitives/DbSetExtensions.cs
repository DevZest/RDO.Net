
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class DbSetExtensions
    {
        //public static DbExpression GetSource<T>(this DbSet<T> dbSet, Column column)
        //    where T : Model, new()
        //{
        //    Check.NotNull(dbSet, nameof(dbSet));
        //    if (column == null || column.ParentModel != dbSet.Model)
        //        return null;

        //    return dbSet.GetSource(column.Ordinal);
        //}

        public static DbFromClause GetFromClause<T>(this DbSet<T> dbSet)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            return dbSet.FromClause;
        }
    }
}
