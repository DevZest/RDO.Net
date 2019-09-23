using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents collection of scalar validation error(s), which is specially optimized for single item collection and immutability.
    /// </summary>
    /// <remarks>
    /// <para><see cref="ScalarValidationError"/> class implements <see cref="IScalarValidationErrors"/>, so a <see cref="ScalarValidationError"/> instance can represent both
    /// the error itself and a single item collection of errors. This can improve performance by avoiding object creation.</para>
    /// <para><see cref="IScalarValidationErrors"/> can be sealed as immutable, any change to <see cref="IScalarValidationErrors"/> may or may not
    /// create a new <see cref="IScalarValidationErrors"/> instance. Consumer of <see cref="IScalarValidationErrors"/> should always assume it's immutable.</para>
    /// <para>Static class <see cref="ScalarValidationErrors"/> is provided to manipulate <see cref="IScalarValidationErrors"/>.</para>
    /// </remarks>
    public interface IScalarValidationErrors : IReadOnlyList<ScalarValidationError>
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
        IScalarValidationErrors Seal();

        /// <summary>
        /// Adds the specified error into this collection.
        /// </summary>
        /// <param name="value">The error to add.</param>
        /// <returns>A new set if there is any modification to current sealed collection; otherwise, the current collection.</returns>
        IScalarValidationErrors Add(ScalarValidationError value);
    }
}
