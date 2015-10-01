
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class DbSetExtensions
    {
        public static Column GetSourceColumn<T>(this DbSet<T> dbSet, Column column)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            if (column == null || column.ParentModel != dbSet.Model)
                return null;

            return dbSet.GetSourceColumn(column.Ordinal);
        }
    }
}
