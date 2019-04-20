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
    }
}
