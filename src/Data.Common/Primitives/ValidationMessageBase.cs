using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public abstract class ValidationMessageBase
    {
        protected ValidationMessageBase(string id, ValidationSeverity severity, string description)
        {
            Check.NotEmpty(description, nameof(description));

            _id = id;
            _description = description;
            _severity = severity;
        }

        private readonly string _id;
        public string Id
        {
            get { return _id; }
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
