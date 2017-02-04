using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public abstract class ValidationMessage<T> : IAbstractValidationMessage
    {
        protected ValidationMessage(string id, ValidationSeverity severity, string description, T source)
        {
            Check.NotEmpty(description, nameof(description));

            Id = id;
            Description = description;
            Severity = severity;
            Source = source;
        }

        public string Id { get; private set; }

        public ValidationSeverity Severity { get; private set; }

        public string Description { get; private set; }

        public T Source { get; private set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
