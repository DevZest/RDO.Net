using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>Represents set of Column(s), which is specially optimized for singleton set and immutability.</summary>
    /// <remarks>
    /// <para><see cref="Column"/> class implements <see cref="IColumns"/>, so a <see cref="Column"/> instance can represent both
    /// the column itself and a singleton set of columns. This can improve performance by avoiding object creation.</para>
    /// <para><see cref="IColumns"/> can be sealed as immutable, any change to <see cref="IColumns"/> may or may not
    /// create a new <see cref="IColumns"/> instance. Consumer of <see cref="IColumns"/> should always assume it's immutable.</para>
    /// <para>Static class <see cref="Columns"/> is provided to manipulate <see cref="IColumns"/>.</para>
    /// </remarks>
    public interface IColumns: IReadOnlyCollection<Column>
    {
        /// <summary>Gets a value indicates whether this set is sealed.</summary>
        bool IsSealed { get; }

        /// <summary>Seals this set as immutable.</summary>
        /// <returns>This set.</returns>
        /// <remarks>After calling Seal(), subsequent change to this set will create a new object.</remarks>
        IColumns Seal();

        /// <summary>Adds the specified Column into this set.</summary>
        /// <param name="value">The Column to add.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumns Add(Column value);

        /// <summary>Removes the specified Column from this set.</summary>
        /// <param name="value">The Column to remove.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumns Remove(Column value);

        /// <summary>Removes all the Columns from this set.</summary>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        IColumns Clear();

        /// <summary>Determines whether this set contains a specified Column.</summary>
        /// <param name="value">The Column to locate in the set.</param>
        /// <returns><see langword="true"/> if the set contains the specified value; otherwise, <see langword="false"/>.</returns>
        bool Contains(Column value);
    }
}
