
namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents the result of inserting scalar values into database table.
    /// </summary>
    public struct InsertScalarResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InsertScalarResult"/>.
        /// </summary>
        /// <param name="success">A value indicates whether the insert is successful.</param>
        /// <param name="identityValue">The newly generated identity value during insert.</param>
        public InsertScalarResult(bool success, long? identityValue)
        {
            Success = success;
            IdentityValue = identityValue;
        }

        /// <summary>
        /// Gets a value indicates whether the insert is successful.
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// Gets the newly generated identity value during insert.
        /// </summary>
        public readonly long? IdentityValue;
    }
}
