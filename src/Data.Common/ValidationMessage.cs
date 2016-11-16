using DevZest.Data.Utilities;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Text;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public class ValidationMessage
    {
        public ValidationMessage(string id, ValidationSeverity severity, IColumnSet columns, string description)
        {
            Check.NotEmpty(description, nameof(description));

            Id = id;
            Severity = severity;
            Columns = columns ?? ColumnSet.Empty;
            Description = description;
        }

        public readonly string Id;

        public readonly ValidationSeverity Severity;

        public readonly string Description;

        public readonly IColumnSet Columns;

        public override string ToString()
        {
            return Description;
        }
    }
}
