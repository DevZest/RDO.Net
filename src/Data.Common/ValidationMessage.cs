using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ValidationMessage : ValidationMessage<IColumnSet>
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description, IColumnSet source)
            : base(id, severity, description, source)
        {
            Check.NotNull(source, nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(Strings.ValidationMessage_EmptySourceColumns, nameof(source));
        }
    }
}
