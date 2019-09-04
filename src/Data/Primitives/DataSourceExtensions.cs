namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension method for DataSource.
    /// </summary>
    public static class DataSourceExtensions
    {
        /// <summary>
        /// Updates the original DataSource.
        /// </summary>
        /// <param name="dataSource">The DataSource.</param>
        /// <param name="originalDataSource">The original DataSource.</param>
        /// <param name="revisionInvariant">Specifies whether this update is revision invariant.</param>
        public static void UpdateOriginalDataSource(this DataSource dataSource, DataSource originalDataSource, bool revisionInvariant)
        {
            dataSource.VerifyNotNull(nameof(dataSource));
            originalDataSource.VerifyNotNull(nameof(originalDataSource));

            dataSource.UpdateOriginalDataSource(originalDataSource, revisionInvariant);
        }
    }
}
