using DevZest.Data.Utilities;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Text;
using DevZest.Data.Primitives;

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

        public override string ToString()
        {
            return Description;
        }

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

        internal void WriteJson(JsonWriter jsonWriter)
        {
            jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(nameof(ValidatorId), ValidatorId.ToString()).WriteComma()
                .WriteNameStringPair(nameof(Severity), Severity.ToString()).WriteComma()
                .WriteNameStringPair(nameof(Columns), Columns.Serialize()).WriteComma()
                .WriteNameStringPair(nameof(Description), Description)
                .WriteEndObject();
        }
    }
}
