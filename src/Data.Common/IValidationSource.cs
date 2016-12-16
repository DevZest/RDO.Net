using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>Represents a set of columns can only be modified by creating a new instance of the set when sealed.</summary>
    public interface IValidationSource<T>: IReadOnlyCollection<T>
        where T : class, IValidationSource<T>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IValidationSource<T> Seal();

        /// <summary>Adds the specified column into this set.</summary>
        /// <param name="value">The column to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Add(T value);

        /// <summary>Removes the specified column from this set.</summary>
        /// <param name="value">The column to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Remove(T value);

        /// <summary>Removes all the columns from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Clear();

        /// <summary>Determines whether this set contains a specified column.</summary>
        /// <param name="value">The column to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(T value);
    }
}
