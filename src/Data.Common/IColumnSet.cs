using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>Represents set of Column(s), which is specially optimized for singleton set.</summary>
    public interface IColumnSet: IReadOnlyCollection<Column>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IColumnSet Seal();

        /// <summary>Adds the specified Column into this set.</summary>
        /// <param name="value">The Column to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Add(Column value);

        /// <summary>Removes the specified Column from this set.</summary>
        /// <param name="value">The Column to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Remove(Column value);

        /// <summary>Removes all the Columns from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Clear();

        /// <summary>Determines whether this set contains a specified Column.</summary>
        /// <param name="value">The Column to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(Column value);
    }
}
