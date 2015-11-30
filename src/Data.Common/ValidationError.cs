
using System.Diagnostics;

namespace DevZest.Data
{
    public struct ValidationError
    {
        internal ValidationError(DataRow dataRow, ValidationRule validationRule)
        {
            Debug.Assert(dataRow != null);
            Debug.Assert(validationRule != null);

            DataRow = dataRow;
            ValidationRule = validationRule;
        }

        public readonly DataRow DataRow;

        public readonly ValidationRule ValidationRule;

        public string ErrorMessage
        {
            get { return ValidationRule == null ? null : ValidationRule.GetErrorMessage(DataRow); }
        }

        public bool IsEmpty
        {
            get { return ValidationRule == null; }
        }
    }
}
