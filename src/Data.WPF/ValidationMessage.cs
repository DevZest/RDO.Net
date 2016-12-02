using System;

namespace DevZest.Data.Windows
{
    public struct ValidationMessage
    {
        public static ValidationMessage Empty = new ValidationMessage();
        public static ValidationMessage Error(string description)
        {
            return new ValidationMessage(string.Empty, description, ValidationSeverity.Error);
        }

        public static ValidationMessage Error(string id, string description)
        {
            return new ValidationMessage(id, description, ValidationSeverity.Error);
        }

        public static ValidationMessage Warning(string description)
        {
            return new ValidationMessage(string.Empty, description, ValidationSeverity.Warning);
        }

        public static ValidationMessage Warning(string id, string description)
        {
            return new ValidationMessage(id, description, ValidationSeverity.Warning);
        }

        public readonly string Id;
        public readonly string Description;
        public readonly ValidationSeverity Severity;

        private ValidationMessage(string id, string description, ValidationSeverity severity)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));
            Id = id;
            Description = description;
            Severity = severity;
        }

        public bool IsEmpty
        {
            get { return Description == null; }
        }

        public bool IsSeverity(ValidationSeverity severity)
        {
            return !IsEmpty && Severity == severity;
        }

        public bool IsError
        {
            get { return IsSeverity(ValidationSeverity.Error); }
        }

        public bool IsWarning
        {
            get { return IsSeverity(ValidationSeverity.Warning); }
        }
    }
}
