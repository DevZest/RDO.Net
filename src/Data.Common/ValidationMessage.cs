using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public struct ValidationMessage<T>
        where T : class, IValidationSource<T>
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description, IValidationSource<T> source)
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

        public readonly IValidationSource<T> Source;

        public override string ToString()
        {
            return Description;
        }
    }
}
