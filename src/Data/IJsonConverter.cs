namespace DevZest.Data
{
    /// <summary>
    /// Converts between column value and <see cref="JsonValue"/>.
    /// </summary>
    public interface IJsonConverter
    {
        /// <summary>
        /// Serializes column value into <see cref="JsonValue"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="rowOrdinal">The ordinal of DataRow.</param>
        /// <returns>The <see cref="JsonValue"/>.</returns>
        JsonValue Serialize(Column column, int rowOrdinal);

        /// <summary>
        /// Deserializes <see cref="JsonValue"/> into column value.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="rowOrdinal">The ordinal of DataRow.</param>
        /// <param name="value">The <see cref="JsonValue"/>.</param>
        void Deserialize(Column column, int rowOrdinal, JsonValue value);
    }
}
