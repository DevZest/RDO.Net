using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents collection of row async validator(s), which is specially optimized for single item collection and immutability.
    /// </summary>
    /// <remarks>
    /// <para><see cref="RowAsyncValidator"/> class implements <see cref="IRowAsyncValidators"/>, so a <see cref="RowAsyncValidator"/> instance can represent both
    /// the validator itself and a single item collection of validators. This can improve performance by avoiding object creation.</para>
    /// <para><see cref="IRowAsyncValidators"/> can be sealed as immutable, any change to <see cref="IRowAsyncValidators"/> may or may not
    /// create a new <see cref="IRowAsyncValidators"/> instance. Consumer of <see cref="IRowAsyncValidators"/> should always assume it's immutable.</para>
    /// <para>Static class <see cref="RowAsyncValidators"/> is provided to manipulate <see cref="IRowAsyncValidators"/>.</para>
    /// </remarks>
    public interface IRowAsyncValidators : IReadOnlyList<RowAsyncValidator>
    {
        /// <summary>
        /// Gets a value indicates whether this collection is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Seals this collection as immutable.
        /// </summary>
        /// <returns>This collection.</returns>
        /// <remarks>After calling Seal(), subsequent change to this collection will create a new object.</remarks>
        IRowAsyncValidators Seal();

        /// <summary>
        /// Adds the specified validator into this collection.
        /// </summary>
        /// <param name="value">The validator to add.</param>
        /// <returns>A new set if there is any modification to current sealed collection; otherwise, the current collection.</returns>
        IRowAsyncValidators Add(RowAsyncValidator value);

        /// <summary>
        /// Gets row async validator by source columns.
        /// </summary>
        /// <param name="sourceColumns">The source columns.</param>
        /// <returns>The result row async validator.</returns>
        RowAsyncValidator this[IColumns sourceColumns] { get; }
    }
}
