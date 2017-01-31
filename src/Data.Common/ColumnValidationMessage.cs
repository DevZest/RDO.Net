using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ColumnValidationMessage : ValidationMessage
    {
        public ColumnValidationMessage(string id, ValidationSeverity severity, string description, IColumnSet source)
            : base(id, severity, description)
        {
            Check.NotNull(source, nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(Strings.ColumnValidationMessage_EmptySource, nameof(source));

            Source = source;
        }

        public readonly IColumnSet Source;
    }
}
