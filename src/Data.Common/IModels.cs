using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IModels : IReadOnlyCollection<Model>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IModels Seal();

        /// <summary>Adds the specified Model into this set.</summary>
        /// <param name="value">The Model to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IModels Add(Model value);

        /// <summary>Removes the specified Model from this set.</summary>
        /// <param name="value">The Model to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IModels Remove(Model value);

        /// <summary>Removes all the Models from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IModels Clear();

        /// <summary>Determines whether this set contains a specified Model.</summary>
        /// <param name="value">The Model to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(Model value);
    }
}
