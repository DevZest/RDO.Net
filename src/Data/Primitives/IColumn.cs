namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents column for reading data values.
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// Gets a value indicates whether this column is readonly for specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns><see langword="true" /> if this column is readonly for specified DataRow, otherwise <see langword="false"/>.</returns>
        bool IsReadOnly(DataRow dataRow);
    }

    /// <summary>
    /// Represents column which can read data value from database reader for DataRow.
    /// </summary>
    /// <typeparam name="TReader">The type of database reader.</typeparam>
    public interface IColumn<in TReader> : IColumn
        where TReader : DbReader
    {
        /// <summary>
        /// Reads data value from database reader for DataRow.
        /// </summary>
        /// <param name="reader">The database reader.</param>
        /// <param name="dataRow">The DataRow.</param>
        void Read(TReader reader, DataRow dataRow);
    }

    /// <summary>
    /// Represents column which can read data value from database reader.
    /// </summary>
    /// <typeparam name="TReader">Type of Database reader.</typeparam>
    /// <typeparam name="TValue">Type of data value.</typeparam>
    public interface IColumn<in TReader, TValue> : IColumn<TReader>
        where TReader : DbReader
    {
        /// <summary>
        /// Gets the data value from database reader.
        /// </summary>
        /// <param name="reader">The database reader.</param>
        /// <returns>The result data value.</returns>
        TValue this[TReader reader] { get; }
    }
}
