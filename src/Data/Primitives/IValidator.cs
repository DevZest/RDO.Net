using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a validator to validate DataRow.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Gets the validator attribute.
        /// </summary>
        IValidatorAttribute Attribute { get; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        Model Model { get; }

        /// <summary>
        /// Gets the source columns for this validator.
        /// </summary>
        IColumns SourceColumns { get; }

        /// <summary>
        /// Validates specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>The validation error, <see langword="null"/> is no error.</returns>
        DataValidationError Validate(DataRow dataRow);
    }
}
