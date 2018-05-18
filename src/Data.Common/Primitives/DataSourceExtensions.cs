using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class DataSourceExtensions
    {
        public static void UpdateOriginalDataSource(this DataSource dataSource, DataSource originalDataSource, bool revisionInvariant)
        {
            Check.NotNull(dataSource, nameof(dataSource));
            Check.NotNull(originalDataSource, nameof(originalDataSource));

            dataSource.UpdateOriginalDataSource(originalDataSource, revisionInvariant);
        }

        public static T GetModel<T>(this DataSet<T> dataSet)
            where T : Model, new()
        {
            return dataSet._;
        }

        public static T GetModel<T>(this DbSet<T> dbSet)
            where T : Model, new()
        {
            return dbSet._;
        }
    }
}
