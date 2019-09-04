namespace DevZest.Data
{
    /// <summary>
    /// Customizes JSON serialization/deserialization.
    /// </summary>
    public interface IJsonCustomizer
    {
        /// <summary>
        /// Determines if specified column is serializable.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns><see langword="true" /> if specified column is serializable, otherwise <see langword="false" />.</returns>
        bool IsSerializable(Column column);

        /// <summary>
        /// Determines if specified column can be deserialized.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns><see langword="true" /> if specified column can be derializable, otherwise <see langword="false" />.</returns>
        bool IsDeserializable(Column column);

        /// <summary>
        /// Gets the <see cref="IJsonConverter"/> for specified column.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns>The <see cref="IJsonConverter"/>.</returns>
        IJsonConverter GetConverter(Column column);
    }
}
