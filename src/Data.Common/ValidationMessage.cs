using DevZest.Data.Utilities;
using System.Collections.Generic;
using System;
using System.Collections;

namespace DevZest.Data
{
    public class ValidationMessage : IReadOnlyList<ValidationMessage>
    {
        public ValidationMessage(ValidatorId validatorId, ValidationSeverity severity, IColumnSet columns, string description)
        {
            Check.NotEmpty(description, nameof(description));

            ValidatorId = validatorId;
            Severity = severity;
            Columns = columns ?? ColumnSet.Empty;
            Description = description;
        }

        public readonly ValidatorId ValidatorId;

        public readonly ValidationSeverity Severity;

        public readonly string Description;

        public readonly IColumnSet Columns;

        #region IReadOnlyList<ValidationMessage>

        int IReadOnlyCollection<ValidationMessage>.Count
        {
            get { return 1; }
        }

        ValidationMessage IReadOnlyList<ValidationMessage>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        IEnumerator<ValidationMessage> IEnumerable<ValidationMessage>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion
    }
}
