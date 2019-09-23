using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents collection of scalar async validator(s), which is specially optimized for single item collection and immutability.
    /// </summary>
    /// <remarks>
    /// <para><see cref="ScalarAsyncValidator"/> class implements <see cref="IScalarAsyncValidators"/>, so a <see cref="ScalarAsyncValidator"/> instance can represent both
    /// the validator itself and a single item collection of validators. This can improve performance by avoiding object creation.</para>
    /// <para><see cref="IScalarAsyncValidators"/> can be sealed as immutable, any change to <see cref="IScalarAsyncValidators"/> may or may not
    /// create a new <see cref="IScalarAsyncValidators"/> instance. Consumer of <see cref="IScalarAsyncValidators"/> should always assume it's immutable.</para>
    /// <para>Static class <see cref="ScalarAsyncValidators"/> is provided to manipulate <see cref="IScalarAsyncValidators"/>.</para>
    /// </remarks>
    public interface IScalarAsyncValidators : IReadOnlyList<ScalarAsyncValidator>
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
        IScalarAsyncValidators Seal();

        /// <summary>
        /// Adds the specified validator into this collection.
        /// </summary>
        /// <param name="value">The validator to add.</param>
        /// <returns>A new set if there is any modification to current sealed collection; otherwise, the current collection.</returns>
        IScalarAsyncValidators Add(ScalarAsyncValidator value);

        /// <summary>
        /// Gets scalar async validator by source scalars.
        /// </summary>
        /// <param name="sourceScalars">The source Scalars.</param>
        /// <returns>The result scalar async validator.</returns>
        ScalarAsyncValidator this[IScalars sourceScalars] { get; }
    }
}
