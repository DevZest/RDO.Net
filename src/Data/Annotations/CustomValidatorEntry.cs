using System;

namespace DevZest.Data.Annotations
{
    public struct CustomValidatorEntry
    {
        public CustomValidatorEntry(Func<DataRow, string> validate, Func<IColumns> getSourceColumns)
        {
            validate.VerifyNotNull(nameof(validate));
            getSourceColumns.VerifyNotNull(nameof(getSourceColumns));

            Validate = validate;
            GetSourceColumns = getSourceColumns;
        }

        public readonly Func<DataRow, string> Validate;

        public readonly Func<IColumns> GetSourceColumns;
    }
}
