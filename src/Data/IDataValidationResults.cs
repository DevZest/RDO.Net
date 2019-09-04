using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a dictionary of <see cref="DataValidationResult"/> objects.
    /// </summary>
    public interface IDataValidationResults : IReadOnlyList<DataValidationResult>
    {
        /// <summary>
        /// Gets a value indicates whether this collection is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Seals this collection.
        /// </summary>
        /// <returns></returns>
        IDataValidationResults Seal();

        /// <summary>
        /// Gets the validation errors for specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>The validation errors of the specified DataRow.</returns>
        IDataValidationErrors this[DataRow dataRow] { get; }

        /// <summary>
        /// Determines whether this collection contains specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns><see langword="true" /> if this collection contains specified DataRow, otherwise <see langword="false" />.</returns>
        bool Contains(DataRow dataRow);

        /// <summary>
        /// Attempts to get the valiation errors that is associated with the specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <param name="value">The validation errors that is associated with the specified DataRow.</param>
        /// <returns><see langword="true" /> if DataRow and its validation error exists, otherwise <see langword="false" />.</returns>
        bool TryGetValue(DataRow dataRow, out IDataValidationErrors value);

        /// <summary>
        /// Adds data validation result into this collection.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>This collection if it's not sealed, otherwise a new collection will be created and returned.</returns>
        IDataValidationResults Add(DataValidationResult value);
    }
}
