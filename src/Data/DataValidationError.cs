using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    public class DataValidationError : ValidationError<IColumns>, IDataValidationErrors
    {
        public DataValidationError(string message, IColumns source)
            : base(message, source)
        {
            source.VerifyNotNull(nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ValidationError_EmptySourceColumns, nameof(source));
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.Create().Write(this).ToString(isPretty);
        }

        public static DataValidationError ParseJson(DataSet dataSet, string json)
        {
            var jsonReader = JsonReader.Create(json);
            var result = jsonReader.ParseValidationMessage(dataSet);
            jsonReader.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        #region IDataValidationErrors

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<DataValidationError>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        DataValidationError IReadOnlyList<DataValidationError>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IDataValidationErrors IDataValidationErrors.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IDataValidationErrors IDataValidationErrors.Add(DataValidationError value)
        {
            value.VerifyNotNull(nameof(value));
            return DataValidationErrors.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<DataValidationError> IEnumerable<DataValidationError>.GetEnumerator()
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
