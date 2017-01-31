using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ValidationMessage : Message
    {
        public ValidationMessage(string id, Severity severity, string description, IColumnSet columns)
            : base(id, description)
        {
            Check.NotNull(columns, nameof(columns));
            if (columns.Count == 0)
                throw new ArgumentException(Strings.ValidationMessage_EmptyColumns, nameof(columns));

            _severity = severity;
            Columns = columns;
        }

        private Severity _severity;
        public sealed override Severity Severity
        {
            get { return _severity; }
        }

        public readonly IColumnSet Columns;
    }
}
