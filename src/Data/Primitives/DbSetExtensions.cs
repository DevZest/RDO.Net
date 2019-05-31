using System;

namespace DevZest.Data.Primitives
{
    public static class DbSetExtensions
    {
        public static DbFromClause GetFromClause(this IDbSet dbSet)
        {
            dbSet.VerifyNotNull(nameof(dbSet));
            return dbSet.FromClause;
        }

        public static DataSet<T> MakeDataSet<T>(this DbSet<T> dbSet, Action<T> initializer = null)
            where T : class, IEntity, new()
        {
            T modelRef = dbSet._.MakeCopy(false);
            modelRef.Initialize(initializer);
            return DataSet<T>.Create(modelRef);
        }
    }
}
