using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>Represents set of object(s) that can be used for the source of validation message, specially optimized for singleton set.</summary>
    public interface IValidationSource<T>: IReadOnlyCollection<T>
        where T : class, IValidationSource<T>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IValidationSource<T> Seal();

        /// <summary>Adds the specified object into this set.</summary>
        /// <param name="value">The object to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Add(T value);

        /// <summary>Removes the specified object from this set.</summary>
        /// <param name="value">The object to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Remove(T value);

        /// <summary>Removes all the objects from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IValidationSource<T> Clear();

        /// <summary>Determines whether this set contains a specified object.</summary>
        /// <param name="value">The object to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(T value);
    }
}
