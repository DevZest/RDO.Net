using System;
using DevZest.Data.Primitives;

namespace DevZest.Data.Windows
{
    public class RowInputError : Message
    {
        public RowInputError(IRowInput rowInput, InputError inputError)
            : base(inputError.Id, inputError.Description)
        {
            RowInput = rowInput;
        }

        public readonly IRowInput RowInput;

        public sealed override Severity Severity
        {
            get { return Severity.Error; }
        }
    }
}
