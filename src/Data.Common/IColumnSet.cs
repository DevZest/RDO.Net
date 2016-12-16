using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>Represents a set of columns can only be modified by creating a new instance of the set when sealed.</summary>
    /// <remarks><see cref="Column"/> class implements this interface for memory efficiency. Use extension methods in <see cref="ColumnSet"/> class
    /// to manipulate set of columns.</remarks> 
    public interface IColumnSet: IReadOnlyCollection<Column>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Ensures this set is sealed.</summary>
        /// <returns>This set.</returns>
        IColumnSet Seal();

        /// <summary>Adds the specified column into this set.</summary>
        /// <param name="value">The column to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Add(Column value);

        /// <summary>Removes the specified column from this set.</summary>
        /// <param name="value">The column to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Remove(Column value);

        /// <summary>Removes all the columns from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumnSet Clear();

        /// <summary>Determines whether this set contains a specified column.</summary>
        /// <param name="value">The column to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(Column value);
    }
}
