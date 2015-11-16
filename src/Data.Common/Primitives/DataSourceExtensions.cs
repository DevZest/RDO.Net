using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class DataSourceExtensions
    {
        public static void UpdateOriginalDataSource(this DataSource dataSource, DataSource originalDataSource, bool isSnapshot)
        {
            Check.NotNull(dataSource, nameof(dataSource));
            Check.NotNull(originalDataSource, nameof(originalDataSource));

            dataSource.UpdateOriginalDataSource(originalDataSource, isSnapshot);
        }
    }
}
