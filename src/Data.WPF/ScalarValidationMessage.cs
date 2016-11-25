using System;

namespace DevZest.Data.Windows
{
    public class ScalarValidationMessage
    {
        public ScalarValidationMessage(string id, ValidationSeverity severity, IScalarSet scalars, string description)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));

            Id = id;
            Severity = severity;
            Scalars = scalars ?? ScalarSet.Empty;
            Description = description;
        }

        public readonly string Id;
        public readonly ValidationSeverity Severity;
        public readonly IScalarSet Scalars;
        public readonly string Description;

        public override string ToString()
        {
            return Description;
        }
    }
}
