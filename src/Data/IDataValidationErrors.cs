using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a collection of <see cref="DataValidationError"/> objects which can be sealed as immutable.
    /// </summary>
    public interface IDataValidationErrors : IReadOnlyList<DataValidationError>
    {
        /// <summary>
        /// Seals this collection as immutable.
        /// </summary>
        /// <returns>This collection for fluent coding.</returns>
        IDataValidationErrors Seal();

        /// <summary>
        /// Adds a <see cref="DataValidationError"/> object into the collection.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>This collection if it's mutable, otherwise a new collection will be created.</returns>
        IDataValidationErrors Add(DataValidationError value);
    }
}
