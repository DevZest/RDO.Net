using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public struct ValidationMessage<T>
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description, T source)
        {
            Check.NotEmpty(description, nameof(description));

            Id = id;
            Severity = severity;
            Source = source;
            Description = description;
        }

        public readonly string Id;

        public readonly ValidationSeverity Severity;

        public readonly string Description;

        public readonly T Source;

        public override string ToString()
        {
            return Description;
        }
    }
}
