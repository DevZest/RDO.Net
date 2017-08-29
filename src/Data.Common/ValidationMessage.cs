using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    public class ValidationMessage : ValidationMessageBase<IColumnSet>, IValidationMessageGroup
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

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ValidationMessage>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ValidationMessage IReadOnlyList<ValidationMessage>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationMessageGroup IValidationMessageGroup.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationMessageGroup IValidationMessageGroup.Add(ValidationMessage value)
        {
            Check.NotNull(value, nameof(value));
            return ValidationMessageGroup.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ValidationMessage> IEnumerable<ValidationMessage>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }
        #endregion
    }
}
