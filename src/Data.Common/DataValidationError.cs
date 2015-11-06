
using System.Diagnostics;

namespace DevZest.Data
{
    public struct DataValidationError
    {
        internal DataValidationError(DataRow dataRow, DataValidation validation)
        {
            Debug.Assert(dataRow != null);
            Debug.Assert(validation != null);

            DataRow = dataRow;
            Validation = validation;
        }

        public readonly DataRow DataRow;

        public readonly DataValidation Validation;

        public string ErrorMessage
        {
            get { return Validation == null ? null : Validation.GetErrorMessage(DataRow); }
        }

        public bool IsEmpty
        {
            get { return Validation == null; }
        }
    }
}
