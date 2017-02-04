using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data
{
    public class ValidationMessage : ValidationMessage<IColumnSet>, IValidationMessageGroup
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description, IColumnSet source)
            : base(id, severity, description, source)
        {
            Check.NotNull(source, nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(Strings.ValidationMessage_EmptySourceColumns, nameof(source));
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public static ValidationMessage ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationMessage(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        #region IValidationMessageGroup

        int IValidationMessageGroup.Count
        {
            get { return 1; }
        }

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

        IValidationMessageGroup IValidationMessageGroup.Seal()
        {
            return this;
        }

        IValidationMessageGroup IValidationMessageGroup.Add(ValidationMessage value)
        {
            Check.NotNull(value, nameof(value));
            return ValidationMessageGroup.New(this, value);
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
