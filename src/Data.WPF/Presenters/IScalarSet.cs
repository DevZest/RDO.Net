using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>Represents set of Scalar(s), which is specially optimized for singleton set.</summary>
    public interface IScalarSet : IReadOnlyCollection<Scalar>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IScalarSet Seal();

        /// <summary>Adds the specified Scalar into this set.</summary>
        /// <param name="value">The Scalar to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IScalarSet Add(Scalar value);

        /// <summary>Removes the specified Scalar from this set.</summary>
        /// <param name="value">The Scalar to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IScalarSet Remove(Scalar value);

        /// <summary>Removes all the Scalar from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IScalarSet Clear();

        /// <summary>Determines whether this set contains a specified Scalar.</summary>
        /// <param name="value">The Scalar to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(Scalar value);
    }
}
