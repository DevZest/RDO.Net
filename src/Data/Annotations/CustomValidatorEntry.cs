using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Contains delegates for custom validator.
    /// </summary>
    public struct CustomValidatorEntry
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CustomValidatorEntry"/>.
        /// </summary>
        /// <param name="validate">The delegate to validate.</param>
        /// <param name="getSourceColumns">The delegate to get source columns.</param>
        public CustomValidatorEntry(Func<DataRow, string> validate, Func<IColumns> getSourceColumns)
        {
            validate.VerifyNotNull(nameof(validate));
            getSourceColumns.VerifyNotNull(nameof(getSourceColumns));

            Validate = validate;
            GetSourceColumns = getSourceColumns;
        }

        /// <summary>
        /// Gets the delegate to validate.
        /// </summary>
        public readonly Func<DataRow, string> Validate;

        /// <summary>
        /// Gets the delegate to get source columns.
        /// </summary>
        public readonly Func<IColumns> GetSourceColumns;
    }
}
