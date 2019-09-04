using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a collection of <see cref="ValidationError"/> objects which can be sealed as immutable.
    /// </summary>
    public interface IValidationErrors : IReadOnlyList<ValidationError>
    {
        /// <summary>
        /// Seals this collection as immutable.
        /// </summary>
        /// <returns>This collection for fluent coding.</returns>
        IValidationErrors Seal();

        /// <summary>
        /// Adds a <see cref="ValidationError"/> object into the collection.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>Current collection if it's mutable, otherwise a new collection will be created and returned.</returns>
        IValidationErrors Add(ValidationError value);
    }
}
