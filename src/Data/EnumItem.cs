namespace DevZest.Data
{
    /// <summary>
    /// Represents an enum item which can be used in a combobox.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct EnumItem<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnumItem{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="description"></param>
        public EnumItem(T value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Gets the enum value.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Gets the description of the enum value.
        /// </summary>
        public readonly string Description;
    }
}
