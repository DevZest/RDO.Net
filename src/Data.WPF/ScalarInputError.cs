using System;
using DevZest.Data.Primitives;

namespace DevZest.Data.Windows
{
    public class ScalarInputError : Message
    {
        public ScalarInputError(IScalarInput scalarInput, InputError inputError)
            : base(inputError.Id, inputError.Description)
        {
            ScalarInput = scalarInput;
        }

        public readonly IScalarInput ScalarInput;

        public sealed override Severity Severity
        {
            get { return Severity.Error; }
        }
    }
}
