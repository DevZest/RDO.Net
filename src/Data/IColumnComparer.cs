namespace DevZest.Data
{
    /// <summary>
    /// Compares DataRow based on column and sorting direction.
    /// </summary>
    public interface IColumnComparer : IDataRowComparer
    {
        /// <summary>
        /// Gets the column for DataRow comparation.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The column.</returns>
        Column GetColumn(Model model);

        /// <summary>
        /// Gets the sorting direction.
        /// </summary>
        SortDirection Direction { get; }
    }
}
