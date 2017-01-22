using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public class ValidationMessage
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description)
        {
            Check.NotEmpty(description, nameof(description));

            Id = id;
            Severity = severity;
            Description = description;
        }

        public readonly string Id;

        public readonly ValidationSeverity Severity;

        public readonly string Description;

        public override string ToString()
        {
            return Description;
        }
    }
}
