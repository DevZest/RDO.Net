using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public abstract class ValidationMessage
    {
        protected ValidationMessage(ValidationSeverity severity, string description)
        {
            Check.NotEmpty(description, nameof(description));

            _description = description;
            _severity = severity;
        }

        private readonly ValidationSeverity _severity;
        public ValidationSeverity Severity
        {
            get { return _severity; }
        }

        private readonly string _description;
        public string Description
        {
            get { return _description; }
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
