namespace DevZest.Data.Primitives
{
    public static class DataSourceExtensions
    {
        public static void UpdateOriginalDataSource(this DataSource dataSource, DataSource originalDataSource, bool revisionInvariant)
        {
            dataSource.VerifyNotNull(nameof(dataSource));
            originalDataSource.VerifyNotNull(nameof(originalDataSource));

            dataSource.UpdateOriginalDataSource(originalDataSource, revisionInvariant);
        }

        public static T GetModel<T>(this DataSet<T> dataSet)
            where T : class, IEntity, new()
        {
            return dataSet._;
        }

        public static T GetModel<T>(this DbSet<T> dbSet)
            where T : class, IEntity, new()
        {
            return dbSet._;
        }
    }
}
