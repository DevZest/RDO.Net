using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a DataSet validation error.
    /// </summary>
    public class DataValidationError : ValidationError<IColumns>, IDataValidationErrors
    {
        /// <summary>
        /// Initializes a new instances of <see cref="DataValidationError"/> class.
        /// </summary>
        /// <param name="message">The validation error message.</param>
        /// <param name="source">The source of the validation error.</param>
        public DataValidationError(string message, IColumns source)
            : base(message, source)
        {
            source.VerifyNotNull(nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ValidationError_EmptySourceColumns, nameof(source));
        }

        /// <summary>
        /// Serializes this validation error messages to JSON string.
        /// </summary>
        /// <param name="isPretty">Determines whether serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer for serialization.</param>
        /// <returns></returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(this).ToString(isPretty);
        }

        /// <summary>
        /// Deserializes JSON string into <see cref="DataValidationError"/>.
        /// </summary>
        /// <param name="dataSet">The DataSet which contains the data for validation.</param>
        /// <param name="json">The JSON string.</param>
        /// <param name="customizer">The customizer for deserialization.</param>
        /// <returns></returns>
        public static DataValidationError ParseJson(DataSet dataSet, string json, IJsonCustomizer customizer = null)
        {
            var jsonReader = JsonReader.Create(json, customizer);
            var result = jsonReader.ParseValidationMessage(dataSet);
            jsonReader.ExpectEof();
            return result;
        }

        /// <inheritdoc />
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
