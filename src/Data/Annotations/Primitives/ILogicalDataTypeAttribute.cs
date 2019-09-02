namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Indicates the attribute specifies the logical data type.
    /// </summary>
    public interface ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        LogicalDataType LogicalDataType { get; }
    }
}
