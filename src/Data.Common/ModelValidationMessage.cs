using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ModelValidationMessage : ValidationMessage
    {
        public ModelValidationMessage(string id, ValidationSeverity severity, string description, IValidationSource<Column> source)
            : base(id, severity, description)
        {
            Check.NotNull(source, nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(Strings.ModelValidationMessage_EmptySource, nameof(source));

            Source = source;
        }

        public readonly IValidationSource<Column> Source;
    }
}
