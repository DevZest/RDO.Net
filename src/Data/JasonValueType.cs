namespace DevZest.Data
{
    /// <summary>
    /// Defines the type of JSON value.
    /// </summary>
    /// <remarks>These values are used by <see cref="Primitives.JsonTokenKind"/>, which has a FlagsAttribute.</remarks>
    public enum JsonValueType
    {
        /// <summary>
        /// Number JSON value.
        /// </summary>
        Number = 0x1,

        /// <summary>
        /// String JSON value.
        /// </summary>
        String = 0x2,

        /// <summary>
        /// True JSON value.
        /// </summary>
        True = 0x4,

        /// <summary>
        /// False JSON value.
        /// </summary>
        False = 0x8,

        /// <summary>
        /// Null JSON value.
        /// </summary>
        Null = 0x10
    }
}
